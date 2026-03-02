using System;
using System.Collections.Generic;

namespace BH.Framework.Utilities.Collections
{
    /// <summary>
    /// 泛型优先队列实现，支持自定义比较器。
    /// 通过二叉堆实现，默认小顶堆。
    /// </summary>
    public class PriorityQueue<T> where T : IComparable<T>
    {
        /// <summary>
        /// 存储堆元素的列表。
        /// </summary>
        private readonly List<T> _elements;

        /// <summary>
        /// 比较器，用于元素优先级比较。
        /// </summary>
        private readonly Comparison<T> _comparison;

        /// <summary>
        /// 队列中元素数量。
        /// </summary>
        public int Count => _elements.Count;

        /// <summary>
        /// 队列是否为空。
        /// </summary>
        public bool IsEmpty => Count == 0;

        /// <summary>
        /// 获取队首元素（优先级最高），不移除。
        /// </summary>
        public T Top
        {
            get
            {
                if (IsEmpty)
                    throw new InvalidOperationException("Priority queue is empty.");
                return _elements[0];
            }
        }

        /// <summary>
        /// 构造函数，支持自定义比较器。
        /// </summary>
        /// <param name="comparison">自定义比较器，若为null则使用默认比较器（小顶堆）。</param>
        public PriorityQueue(Comparison<T> comparison = null)
        {
            _elements = new List<T>();
            _comparison = comparison ?? ((a, b) => a.CompareTo(b));
        }

        /// <summary>
        /// 构造函数，从现有集合初始化优先队列。
        /// </summary>
        /// <param name="collection">初始元素集合。</param>
        /// <param name="comparison">自定义比较器，若为null则使用默认比较器。</param>
        public PriorityQueue(IEnumerable<T> collection, Comparison<T> comparison = null)
        {
            _elements = new List<T>(collection);
            _comparison = comparison ?? ((a, b) => a.CompareTo(b));
            BuildHeap();
        }

        /// <summary>
        /// 建堆操作，将当前元素列表调整为堆结构。
        /// </summary>
        private void BuildHeap()
        {
            for (int i = (Count - 2) / 2; i >= 0; i--)
                ShiftDown(i);
        }

        /// <summary>
        /// 向下调整堆（下滤），保持堆性质。
        /// </summary>
        /// <param name="index">需要下滤的节点索引。</param>
        private void ShiftDown(int index)
        {
            int leftChild = 2 * index + 1;
            while (leftChild < Count)
            {
                int rightChild = leftChild + 1;
                int topChild = leftChild;

                // 选择左,右孩子中优先级更高的
                if (rightChild < Count && _comparison(_elements[rightChild], _elements[leftChild]) < 0)
                    topChild = rightChild;

                // 如果当前节点优先级已高于孩子，则无需调整
                if (_comparison(_elements[topChild], _elements[index]) >= 0)
                    break;

                Swap(index, topChild);
                index = topChild;
                leftChild = 2 * index + 1;
            }
        }

        /// <summary>
        /// 向上调整堆（上滤），保持堆性质。
        /// </summary>
        /// <param name="index">需要上滤的节点索引。</param>
        private void ShiftUp(int index)
        {
            while (index > 0)
            {
                var parent = (index - 1) / 2;
                // 如果当前节点优先级不低于父节点，则无需调整
                if (_comparison(_elements[index], _elements[parent]) >= 0)
                    break;

                Swap(index, parent);
                index = parent;
            }
        }

        /// <summary>
        /// 交换堆中两个元素的位置。
        /// </summary>
        /// <param name="i">第一个元素索引。</param>
        /// <param name="j">第二个元素索引。</param>
        private void Swap(int i, int j)
        {
            (_elements[i], _elements[j]) = (_elements[j], _elements[i]);
        }

        /// <summary>
        /// 向优先队列中插入一个元素。
        /// </summary>
        /// <param name="value">要插入的元素。</param>
        public void Push(T value)
        {
            _elements.Add(value);
            ShiftUp(Count - 1);
        }

        /// <summary>
        /// 移除并返回队首元素（优先级最高）。
        /// </summary>
        /// <returns>队首元素。</returns>
        public T Pop()
        {
            if (IsEmpty)
                throw new InvalidOperationException("Priority queue is empty.");

            var result = _elements[0];
            var lastIndex = Count - 1;
            _elements[0] = _elements[lastIndex];
            _elements.RemoveAt(lastIndex);

            if (Count > 0)
                ShiftDown(0);

            return result;
        }

        /// <summary>
        /// 判断队列中是否包含指定元素。
        /// </summary>
        /// <param name="value">要查找的元素。</param>
        /// <returns>是否包含该元素。</returns>
        public bool Contains(T value)
        {
            return _elements.Contains(value);
        }

        /// <summary>
        /// 移除队列中的指定元素。
        /// </summary>
        /// <param name="value">要移除的元素。</param>
        /// <returns>是否成功移除。</returns>
        public bool Remove(T value)
        {
            var index = _elements.IndexOf(value);
            if (index == -1)
                return false;

            int lastIndex = Count - 1;
            _elements[index] = _elements[lastIndex];
            _elements.RemoveAt(lastIndex);

            if (index >= lastIndex) return true;

            ShiftDown(index);
            if (index < Count && _elements[index].Equals(_elements[lastIndex]))
                ShiftUp(index);

            return true;
        }

        /// <summary>
        /// 清空优先队列。
        /// </summary>
        public void Clear()
        {
            _elements.Clear();
        }

        /// <summary>
        /// 获取队列中所有元素的有序列表（不改变队列本身）。
        /// </summary>
        /// <returns>有序元素列表。</returns>
        public IEnumerable<T> GetSortedElements()
        {
            var copy = new List<T>(_elements);
            copy.Sort(_comparison);
            return copy;
        }
    }
}