using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;

namespace MauiCrudApp.Common.Utilities;

public class ChangeTracker : IDisposable
{
    private readonly Type _type;
    private object _instance;
    private ObservableObject _parent;
    private string _propertyName;
    private bool _disposed;
    private Action _onChanged;
    private bool _hasChanges;

    public bool HasChanges
    {
        get => _hasChanges;
        private set => _hasChanges = value;
    }

    // Constructor initializes the type and instance to track
    public ChangeTracker(Type type, object instance)
    {
        _type = type ?? throw new ArgumentNullException(nameof(type));
        _instance = instance ?? throw new ArgumentNullException(nameof(instance));
    }

    // Starts tracking changes for the instance, invoking onChanged on any change if provided
    public void Track(Action? onChanged = null)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ChangeTracker));

        if (_instance is ObservableObject observable)
        {
            StartTracking(observable, onChanged);
            _onChanged = onChanged;
        }
    }

    // Tracks changes for a specific property of a parent object, invoking onChanged on any change if provided
    public void TrackProperty(ObservableObject parent, string propertyName, Action? onChanged = null)
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

    // Saves current state and resets HasChanges
    public void Save()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ChangeTracker));

        _hasChanges = false;
        // No state tracking, so nothing else to save
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
            _hasChanges = true;
            _onChanged?.Invoke();
        }
    }

    // Starts tracking changes for an ObservableObject
    private void StartTracking(ObservableObject observable, Action? onChanged)
    {
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
        observable.PropertyChanged += Instance_PropertyChanged;
    }

    // Handles direct property changes of the tracked instance
    private void Instance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        var prop = _type.GetProperty(e.PropertyName);
        if (prop?.GetCustomAttributes(typeof(TrackChangesAttribute), false).Any() == true)
        {
            _hasChanges = true;
            _onChanged?.Invoke();
        }
        var value = prop?.GetValue(sender);
        if (value != null)
        {
            UnsubscribeFromProperty(value); // Unsubscribe from old value
            SubscribeToProperty(value, _onChanged); // Subscribe to new value
        }
    }

    // Subscribes to all TrackChanges properties of an ObservableObject
    private void SubscribeToProperties(ObservableObject observable, Action? onChanged)
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

    // Subscribes to a single property value (ObservableObject and/or INotifyCollectionChanged)
    private void SubscribeToProperty(object value, Action? onChanged)
    {
        if (value is ObservableObject nestedObj)
        {
            nestedObj.PropertyChanged -= NestedObj_PropertyChanged;
            nestedObj.PropertyChanged += NestedObj_PropertyChanged;
            SubscribeToProperties(nestedObj, onChanged);
        }
        if (value is INotifyCollectionChanged nestedCollection)
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
        if (value is INotifyCollectionChanged nestedCollection)
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
        _hasChanges = true;
        _onChanged?.Invoke();
    }

    // Handles property changes for nested objects
    private void NestedObj_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        _hasChanges = true;
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
        _hasChanges = true;
        _onChanged?.Invoke();
    }

    // Handles property changes for collection items
    private void ItemObj_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        _hasChanges = true;
        _onChanged?.Invoke();
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

            var properties = _type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetCustomAttributes(typeof(TrackChangesAttribute), false).Any());

            foreach (var prop in properties)
            {
                var value = prop.GetValue(observable);
                if (value != null)
                {
                    UnsubscribeFromProperty(value);
                }
            }
        }

        _disposed = true;
    }
}
