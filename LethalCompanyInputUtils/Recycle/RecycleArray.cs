using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LethalCompanyInputUtils.Recycle;

internal class RecycleArray<T> : IEnumerable<T>
    where T : Component
{
    private readonly List<T> _pool;
    private int _topIndex;
    private int _bottomIndex;

    public int Size => _pool.Count;
    public int TopDataIndex { get; private set; }
    public int BottomDataIndex { get; private set; }

    internal RecycleArray()
    {
        _pool = new List<T>();
        _topIndex = 0;
        _bottomIndex = 0;
        TopDataIndex = 0;
        BottomDataIndex = 0;
    }

    public void Add(T item)
    {
        _pool.Add(item);

        _bottomIndex = _pool.Count - 1;
        BottomDataIndex = _pool.Count - 1;
    }

    public T RecycleTopToBottom()
    {
        _bottomIndex = _topIndex;
        _topIndex = (_topIndex + 1) % _pool.Count;

        TopDataIndex++;
        BottomDataIndex++;

        return _pool[_bottomIndex];
    }

    public T RecycleBottomToTop()
    {
        _topIndex = _bottomIndex;
        _bottomIndex = (_bottomIndex - 1 + _pool.Count) % _pool.Count;

        TopDataIndex--;
        BottomDataIndex--;

        return _pool[_topIndex];
    }

    public T GetTop()
    {
        return _pool[_topIndex];
    }

    public T GetBottom()
    {
        return _pool[_bottomIndex];
    }

    public void Clear()
    {
        foreach (var component in _pool)
            Object.DestroyImmediate(component.gameObject);
        
        _pool.Clear();

        _topIndex = 0;
        _bottomIndex = 0;
        TopDataIndex = 0;
        BottomDataIndex = 0;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return new Enumerator(_pool.ToArray(), _topIndex);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    internal class Enumerator : IEnumerator<T>
    {
        private readonly T[] _items;
        private readonly int _topIndex;
        private int _recyclePos;
        private int _arrayPos = -1;

        internal Enumerator(T[] items, int topIndex)
        {
            _items = items;
            _topIndex = topIndex;

            _recyclePos = _topIndex - 1;
        }

        public bool MoveNext()
        {
            _arrayPos++;
            _recyclePos++;

            if (_recyclePos >= _items.Length)
                _recyclePos = 0;

            return _arrayPos < _items.Length;
        }

        public void Reset()
        {
            _recyclePos = _topIndex;
            _arrayPos = -1;
        }

        public T Current
        {
            get
            {
                try
                {
                    return _items[_recyclePos];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        object IEnumerator.Current => Current;
        
        public void Dispose() { }
    }
}