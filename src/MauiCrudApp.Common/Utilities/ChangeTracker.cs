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

    public bool HasChanges => _hasChanges;

    public ChangeTracker(Type type, object instance)
    {
        _type = type ?? throw new ArgumentNullException(nameof(type));
        _instance = instance ?? throw new ArgumentNullException(nameof(instance));
    }

    public void Track(Action onChanged)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ChangeTracker));

        if (_instance is ObservableObject observable)
        {
            //SaveInitialValues();
            //observable.PropertyChanged += Instance_PropertyChanged;
            //_onChanged = onChanged;
            StartTracking(observable, onChanged);
            _onChanged = onChanged;
        }
    }

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

    public void Save()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ChangeTracker));

        SaveInitialValues();
        _hasChanges = false;
    }

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

    private bool HasChangesInternal()
    {
        return _initialValues.Any(kvp =>
        {
            var currentValue = _type.GetProperty(kvp.Key)?.GetValue(_instance);
            var initialValue = kvp.Value;
            return !DeepEquals(currentValue, initialValue);
        });
    }

    private Action _onChanged;

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

    private void Instance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (_initialValues.ContainsKey(e.PropertyName))
        {
            _hasChanges = HasChangesInternal();
            _onChanged?.Invoke();
        }
    }

    private void StartTracking(ObservableObject observable, Action onChanged)
    {
        SaveInitialValues();

        // INotifyCollectionChangedをチェック
        if (observable is INotifyCollectionChanged collection)
        {
            collection.CollectionChanged += Collection_CollectionChanged;
        }

        observable.PropertyChanged += (s, e) =>
        {
            if (_initialValues.ContainsKey(e.PropertyName))
            {
                _hasChanges = HasChangesInternal();
                onChanged?.Invoke();
            }
            else if (_initialValues.Any(kvp => kvp.Key == e.PropertyName && kvp.Value != null))
            {
                var prop = _type.GetProperty(e.PropertyName);
                var value = prop?.GetValue(observable);
                if (value is ObservableObject nestedObj)
                {
                    nestedObj.PropertyChanged += NestedObj_PropertyChanged;
                }
                else if (value is INotifyCollectionChanged nestedCollection)
                {
                    nestedCollection.CollectionChanged += NestedCollection_CollectionChanged;
                    if (nestedCollection is IEnumerable enumerable)
                    {
                        foreach (var item in enumerable)
                        {
                            if (item is ObservableObject itemObj)
                            {
                                itemObj.PropertyChanged += ItemObj_PropertyChanged;
                            }
                        }
                    }
                }
            }
        };
    }

    private void Collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        _hasChanges = HasChangesInternal();
        _onChanged?.Invoke();
    }

    private void NestedObj_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        _hasChanges = HasChangesInternal();
        _onChanged?.Invoke();
    }

    private void NestedCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        _hasChanges = HasChangesInternal();
        _onChanged?.Invoke();
    }

    private void ItemObj_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        _hasChanges = HasChangesInternal();
        _onChanged?.Invoke();
    }

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
                if (value is ObservableObject nestedObj)
                {
                    nestedObj.PropertyChanged -= NestedObj_PropertyChanged;
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
        }

        _disposed = true;
    }
}

/*
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;

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

public bool HasChanges => _hasChanges;

public ChangeTracker(Type type, object instance)
{
    _type = type ?? throw new ArgumentNullException(nameof(type));
    _instance = instance ?? throw new ArgumentNullException(nameof(instance));
}

public void Track(Action onChanged)
{
    if (_disposed)
        throw new ObjectDisposedException(nameof(ChangeTracker));

    if (_instance is ObservableObject observable)
    {
        SaveInitialValues();
        observable.PropertyChanged += Instance_PropertyChanged;
        _onChanged = onChanged;
    }
}

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

public void Save()
{
    if (_disposed)
        throw new ObjectDisposedException(nameof(ChangeTracker));

    SaveInitialValues();
    _hasChanges = false;
}

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

private bool HasChangesInternal()
{
    return _initialValues.Any(kvp =>
    {
        var currentValue = _type.GetProperty(kvp.Key)?.GetValue(_instance);
        var initialValue = kvp.Value;
        return !DeepEquals(currentValue, initialValue);
    });
}

private Action _onChanged;

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

private void Instance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
{
    if (_initialValues.ContainsKey(e.PropertyName))
    {
        _hasChanges = HasChangesInternal();
        _onChanged?.Invoke();
    }
}

private void StartTracking(ObservableObject observable, Action onChanged)
{
    SaveInitialValues();

    if (observable is ObservableCollection<object> collection)
    {
        ((INotifyCollectionChanged)collection).CollectionChanged += Collection_CollectionChanged;
    }
    else if (observable is ObservableDictionary<object, object> dict)
    {
        ((INotifyCollectionChanged)dict).CollectionChanged += Collection_CollectionChanged;
    }

    observable.PropertyChanged += (s, e) =>
    {
        if (_initialValues.ContainsKey(e.PropertyName))
        {
            _hasChanges = HasChangesInternal();
            onChanged?.Invoke();
        }
        else if (_initialValues.Any(kvp => kvp.Key == e.PropertyName && kvp.Value != null))
        {
            var prop = _type.GetProperty(e.PropertyName);
            var value = prop?.GetValue(observable);
            if (value is ObservableObject nestedObj)
            {
                nestedObj.PropertyChanged += NestedObj_PropertyChanged;
            }
            else if (value is ObservableCollection<object> nestedCollection)
            {
                ((INotifyCollectionChanged)nestedCollection).CollectionChanged += NestedCollection_CollectionChanged;
                foreach (var item in nestedCollection)
                {
                    if (item is ObservableObject itemObj)
                    {
                        itemObj.PropertyChanged += ItemObj_PropertyChanged;
                    }
                }
            }
            else if (value is ObservableDictionary<object, object> nestedDict)
            {
                ((INotifyCollectionChanged)nestedDict).CollectionChanged += NestedDict_CollectionChanged;
                foreach (var val in nestedDict.Values)
                {
                    if (val is ObservableObject dictObj)
                    {
                        dictObj.PropertyChanged += DictObj_PropertyChanged;
                    }
                }
            }
        }
    };
}

private void Collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
{
    _hasChanges = HasChangesInternal();
    _onChanged?.Invoke();
}

private void NestedObj_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
{
    _hasChanges = HasChangesInternal();
    _onChanged?.Invoke();
}

private void NestedCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
{
    _hasChanges = HasChangesInternal();
    _onChanged?.Invoke();
}

private void ItemObj_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
{
    _hasChanges = HasChangesInternal();
    _onChanged?.Invoke();
}

private void DictObj_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
{
    _hasChanges = HasChangesInternal();
    _onChanged?.Invoke();
}

private void NestedDict_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
{
    _hasChanges = HasChangesInternal();
    _onChanged?.Invoke();
}

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

        if (observable is ObservableCollection<object> collection)
        {
            ((INotifyCollectionChanged)collection).CollectionChanged -= Collection_CollectionChanged;
        }
        else if (observable is ObservableDictionary<object, object> dict)
        {
            ((INotifyCollectionChanged)dict).CollectionChanged -= Collection_CollectionChanged;
        }

        foreach (var kvp in _initialValues)
        {
            var prop = _type.GetProperty(kvp.Key);
            var value = prop?.GetValue(observable);
            if (value is ObservableObject nestedObj)
            {
                nestedObj.PropertyChanged -= NestedObj_PropertyChanged;
            }
            else if (value is ObservableCollection<object> nestedCollection)
            {
                ((INotifyCollectionChanged)nestedCollection).CollectionChanged -= NestedCollection_CollectionChanged;
                foreach (var item in nestedCollection)
                {
                    if (item is ObservableObject itemObj)
                    {
                        itemObj.PropertyChanged -= ItemObj_PropertyChanged;
                    }
                }
            }
            else if (value is ObservableDictionary<object, object> nestedDict)
            {
                ((INotifyCollectionChanged)nestedDict).CollectionChanged -= NestedDict_CollectionChanged;
                foreach (var val in nestedDict.Values)
                {
                    if (val is ObservableObject dictObj)
                    {
                        dictObj.PropertyChanged -= DictObj_PropertyChanged;
                    }
                }
            }
        }
    }

    _disposed = true;
}
}
*/