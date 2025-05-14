using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;

namespace MauiCrudApp.Common.Utilities;

public class ChangeTracker : IDisposable
{
    private readonly Type _type;
    private object _instance;
    private readonly Dictionary<string, object> _initialValues = new();
    private bool _hasChanges;
    private ObservableObject _parent;
    private string _propertyName;
    private bool _disposed;
    private Action _onChanged;

    // Gets whether any tracked properties have changed
    public bool HasChanges => _hasChanges;

    // Constructor initializes the type and instance to track
    public ChangeTracker(Type type, object instance)
    {
        _type = type ?? throw new ArgumentNullException(nameof(type));
        _instance = instance ?? throw new ArgumentNullException(nameof(instance));
    }

    // Starts tracking changes for the instance
    public void Track(Action onChanged)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ChangeTracker));

        if (_instance is ObservableObject observable)
        {
            StartTracking(observable, onChanged);
            _onChanged = onChanged;
        }
    }

    // Tracks changes for a specific property of a parent object
    public void TrackProperty(ObservableObject parent, string propertyName, Action onChanged)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ChangeTracker));

        _parent = parent ?? throw new ArgumentNullException(nameof(parent));
        _propertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));

        if (_instance is ObservableObject observable)
        {
            StartTracking(observable, onChanged);
        }

        parent.PropertyChanged += Parent_PropertyChanged;
        _onChanged = onChanged;
    }

    // Saves current property values as the baseline
    public void Save()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ChangeTracker));

        SaveInitialValues();
        _hasChanges = false;
    }

    // Saves initial values of tracked properties
    private void SaveInitialValues()
    {
        _initialValues.Clear();
        var properties = _type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetCustomAttributes(typeof(TrackChangesAttribute), false).Any());

        foreach (var prop in properties)
        {
            var value = prop.GetValue(_instance);
            _initialValues[prop.Name] = CloneValue(value);
        }
    }

    // Checks if any tracked properties have changed
    private bool HasChangesInternal()
    {
        return _initialValues.Any(kvp =>
        {
            var currentValue = _type.GetProperty(kvp.Key)?.GetValue(_instance);
            var initialValue = kvp.Value;
            return !DeepEquals(currentValue, initialValue);
        });
    }

    // Handles property changes of the parent object
    private void Parent_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == _propertyName)
        {
            _instance = _type.GetProperty(_propertyName)?.GetValue(_parent);
            if (_instance is ObservableObject newObservable)
            {
                StartTracking(newObservable, _onChanged);
            }
            _hasChanges = HasChangesInternal();
            _onChanged?.Invoke();
        }
    }

    // Handles direct property changes of the tracked instance
    private void Instance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (_initialValues.ContainsKey(e.PropertyName))
        {
            _hasChanges = HasChangesInternal();
            _onChanged?.Invoke();
        }
    }

    // Starts tracking changes for an ObservableObject
    private void StartTracking(ObservableObject observable, Action onChanged)
    {
        SaveInitialValues();

        // Subscribe to collection changes if the object is a collection
        if (observable is INotifyCollectionChanged collection)
        {
            collection.CollectionChanged -= Collection_CollectionChanged;
            collection.CollectionChanged += Collection_CollectionChanged;
        }

        // Recursively subscribe to all TrackChanges properties
        SubscribeToProperties(observable, onChanged);

        // Handle property changes to update subscriptions dynamically
        observable.PropertyChanged -= Instance_PropertyChanged;
        observable.PropertyChanged += (s, e) =>
        {
            if (_initialValues.ContainsKey(e.PropertyName))
            {
                _hasChanges = HasChangesInternal();
                onChanged?.Invoke();
            }
            var prop = _type.GetProperty(e.PropertyName);
            var value = prop?.GetValue(observable);
            if (value != null)
            {
                UnsubscribeFromProperty(value); // Unsubscribe from old value
                SubscribeToProperty(value, onChanged); // Subscribe to new value
            }
        };
    }

    // Subscribes to all TrackChanges properties of an ObservableObject
    private void SubscribeToProperties(ObservableObject observable, Action onChanged)
    {
        var properties = observable.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetCustomAttributes(typeof(TrackChangesAttribute), false).Any());

        foreach (var prop in properties)
        {
            var value = prop.GetValue(observable);
            if (value != null)
            {
                SubscribeToProperty(value, onChanged);
            }
        }
    }

    // Subscribes to a single property value (ObservableObject or INotifyCollectionChanged)
    private void SubscribeToProperty(object value, Action onChanged)
    {
        if (value is ObservableObject nestedObj)
        {
            nestedObj.PropertyChanged -= NestedObj_PropertyChanged;
            nestedObj.PropertyChanged += NestedObj_PropertyChanged;
            // Recursively subscribe to nested object's properties
            SubscribeToProperties(nestedObj, onChanged);
        }
        else if (value is INotifyCollectionChanged nestedCollection)
        {
            nestedCollection.CollectionChanged -= NestedCollection_CollectionChanged;
            nestedCollection.CollectionChanged += NestedCollection_CollectionChanged;
            if (nestedCollection is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    if (item is ObservableObject itemObj)
                    {
                        itemObj.PropertyChanged -= ItemObj_PropertyChanged;
                        itemObj.PropertyChanged += ItemObj_PropertyChanged;
                        SubscribeToProperties(itemObj, onChanged);
                    }
                }
            }
        }
    }

    // Unsubscribes from a property value
    private void UnsubscribeFromProperty(object value)
    {
        if (value is ObservableObject nestedObj)
        {
            nestedObj.PropertyChanged -= NestedObj_PropertyChanged;
            var properties = nestedObj.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetCustomAttributes(typeof(TrackChangesAttribute), false).Any());

            foreach (var prop in properties)
            {
                var propValue = prop.GetValue(nestedObj);
                if (propValue != null)
                {
                    UnsubscribeFromProperty(propValue);
                }
            }
        }
        else if (value is INotifyCollectionChanged nestedCollection)
        {
            nestedCollection.CollectionChanged -= NestedCollection_CollectionChanged;
            if (nestedCollection is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    if (item is ObservableObject itemObj)
                    {
                        itemObj.PropertyChanged -= ItemObj_PropertyChanged;
                    }
                }
            }
        }
    }

    // Handles collection changes for the top-level object
    private void Collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (var item in e.NewItems)
            {
                if (item is ObservableObject itemObj)
                {
                    itemObj.PropertyChanged -= ItemObj_PropertyChanged;
                    itemObj.PropertyChanged += ItemObj_PropertyChanged;
                    SubscribeToProperties(itemObj, _onChanged);
                }
            }
        }
        _hasChanges = HasChangesInternal();
        _onChanged?.Invoke();
    }

    // Handles property changes for nested objects
    private void NestedObj_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        _hasChanges = HasChangesInternal();
        _onChanged?.Invoke();
    }

    // Handles collection changes for nested collections
    private void NestedCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (var item in e.NewItems)
            {
                if (item is ObservableObject itemObj)
                {
                    itemObj.PropertyChanged -= ItemObj_PropertyChanged;
                    itemObj.PropertyChanged += ItemObj_PropertyChanged;
                    SubscribeToProperties(itemObj, _onChanged);
                }
            }
        }
        _hasChanges = HasChangesInternal();
        _onChanged?.Invoke();
    }

    // Handles property changes for collection items
    private void ItemObj_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        _hasChanges = HasChangesInternal();
        _onChanged?.Invoke();
    }

    // Clones a value for initial state tracking
    private object CloneValue(object value, Dictionary<object, object> clonedObjects = null)
    {
        if (value == null)
            return null;

        clonedObjects ??= new Dictionary<object, object>();
        if (clonedObjects.ContainsKey(value))
            return clonedObjects[value];

        if (value is ObservableObject observable)
        {
            var newInstance = Activator.CreateInstance(value.GetType());
            clonedObjects[value] = newInstance;

            var properties = value.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetCustomAttributes(typeof(TrackChangesAttribute), false).Any() && p.CanRead && p.CanWrite);

            foreach (var prop in properties)
            {
                var propValue = prop.GetValue(value);
                var clonedValue = CloneValue(propValue, clonedObjects);
                prop.SetValue(newInstance, clonedValue);
            }
            return newInstance;
        }
        else if (value is IList list)
        {
            var listType = typeof(List<>).MakeGenericType(list.GetType().GetGenericArguments()[0]);
            var newList = (IList)Activator.CreateInstance(listType);
            clonedObjects[value] = newList;

            foreach (var item in list)
            {
                newList.Add(CloneValue(item, clonedObjects));
            }
            return newList;
        }
        else if (value is IDictionary dict)
        {
            var dictType = dict is ObservableDictionary<object, object>
                ? dict.GetType()
                : typeof(ObservableDictionary<,>).MakeGenericType(dict.GetType().GetGenericArguments());
            var newDict = (IDictionary)Activator.CreateInstance(dictType);
            clonedObjects[value] = newDict;

            foreach (var key in dict.Keys)
            {
                newDict.Add(key, CloneValue(dict[key], clonedObjects));
            }
            return newDict;
        }

        return value;
    }

    // Deeply compares two objects for equality
    private bool DeepEquals(object a, object b)
    {
        if (ReferenceEquals(a, b))
            return true;
        if (a == null || b == null)
            return false;

        if (a is ObservableObject objA && b is ObservableObject objB)
        {
            var properties = a.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetCustomAttributes(typeof(TrackChangesAttribute), false).Any());

            return !properties.Any(p =>
            {
                var valA = p.GetValue(a);
                var valB = p.GetValue(b);
                return !DeepEquals(valA, valB);
            });
        }
        else if (a is IList listA && b is IList listB)
        {
            if (listA.Count != listB.Count)
                return false;
            for (int i = 0; i < listA.Count; i++)
            {
                if (!DeepEquals(listA[i], listB[i]))
                    return false;
            }
            return true;
        }
        else if (a is IDictionary dictA && b is IDictionary dictB)
        {
            if (dictA.Keys.Count != dictB.Keys.Count)
                return false;
            foreach (var key in dictA.Keys)
            {
                if (!dictB.Contains(key) || !DeepEquals(dictA[key], dictB[key]))
                    return false;
            }
            return true;
        }

        return Equals(a, b);
    }

    // Disposes the tracker and unsubscribes all events
    public void Dispose()
    {
        if (_disposed)
            return;

        if (_parent != null)
        {
            _parent.PropertyChanged -= Parent_PropertyChanged;
        }

        if (_instance is ObservableObject observable)
        {
            observable.PropertyChanged -= Instance_PropertyChanged;

            if (observable is INotifyCollectionChanged collection)
            {
                collection.CollectionChanged -= Collection_CollectionChanged;
            }

            foreach (var kvp in _initialValues)
            {
                var prop = _type.GetProperty(kvp.Key);
                var value = prop?.GetValue(observable);
                if (value != null)
                {
                    UnsubscribeFromProperty(value);
                }
            }
        }

        _disposed = true;
    }
}
