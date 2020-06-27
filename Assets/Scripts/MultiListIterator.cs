using System.Collections;
using System.Collections.Generic;

public class MultiListIterator<T> : IEnumerator<T>, IEnumerable<T>
{
    private List<List<T>> lists;
    private IEnumerator<List<T>> listEnumerator = null;
    private IEnumerator<T> elementEnumerator = null;

    public MultiListIterator()
    {
        this.lists = new List<List<T>>();
    }

    public void AddList(List<T> list)
    {
        lists.Add(list);
        listEnumerator = lists.GetEnumerator();
        listEnumerator.MoveNext();
        elementEnumerator = listEnumerator.Current.GetEnumerator();
    }

    public void RemoveList(List<T> list)
    {
        lists.Remove(list);
        if (lists.Count > 0)
        {
            listEnumerator = lists.GetEnumerator();
            listEnumerator.MoveNext();
            elementEnumerator = listEnumerator.Current.GetEnumerator();
        }
    }

    public T Current => elementEnumerator.Current;

    object IEnumerator.Current => Current;

    public bool MoveNext()
    {
        if (lists.Count == 0) return false;
        if (elementEnumerator.MoveNext()) return true; // more elements in curent list
        while (listEnumerator.MoveNext() && listEnumerator.Current.Count == 0) { }
        // we have no more lists OR we found not-empty one
        if (listEnumerator.Current == null) return false;
        if (listEnumerator.Current.Count > 0)
        {
            elementEnumerator = listEnumerator.Current.GetEnumerator();
            elementEnumerator.MoveNext();
            return true;
        }
        return false;
    }

    public void Reset()
    {
        if (lists.Count > 0)
        {
            listEnumerator = lists.GetEnumerator();
            listEnumerator.MoveNext();
            elementEnumerator = listEnumerator.Current.GetEnumerator();
        }
    }

    public void Dispose()
    {
    }

    public IEnumerator<T> GetEnumerator()
    {
        return this;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}