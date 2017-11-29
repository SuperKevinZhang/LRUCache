    /// <summary>
    /// LRU缓存 Static缓存
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class LRUCache<TValue> : IEnumerable<KeyValuePair<string, TValue>>
    {
        private long ageToDiscard = 0;  //淘汰的年龄起点
        private long currentAge = 0;        //当前缓存最新年龄
        private int maxSize = 0;          //缓存最大容量

        //使用ConcurrentDictionary来作为我们的缓存容器，并能保证线程安全
        private readonly ConcurrentDictionary<string, TrackValue> cache;

        private TimeSpan maxTime;

        //实现IEnumerable方法一
        public System.Collections.Generic.IEnumerator<KeyValuePair<string, TValue>> GetEnumerator()
        {
            return this.GetEnumerator();
        }
        //实现IEnumerable方法二
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();//IEnumerator<T>继承自IEnumerator
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="maxKeySize">缓存最大size</param>
        public LRUCache(int maxKeySize)
        {
            cache = new ConcurrentDictionary<string, TrackValue>();
            maxSize = maxKeySize;
        }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="maxKeySize">缓存最大size</param>
        /// <param name="maxExpireTime">时间跨度</param>
        public LRUCache(int maxKeySize, TimeSpan maxExpireTime)
        {
            cache = new ConcurrentDictionary<string, TrackValue>();
            maxSize = maxKeySize;
            maxTime = maxExpireTime;
        }

        /// <summary>
        /// 新增缓存
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        public void Set(string key, TValue value)
        {
            Adjust(key);
            var result = new TrackValue(this, value);
            cache.AddOrUpdate(key, result, (k, o) => result);
        }
        /// <summary>
        /// 跟踪值
        /// </summary>
        public class TrackValue
        {
            public readonly TValue Value;
            public long Age;

            //TrackValue增加创建时间和过期时间
            public readonly DateTime CreateTime;
            public readonly TimeSpan ExpireTime;

            public TrackValue(LRUCache<TValue> lv, TValue tv)
            {
                Age = Interlocked.Increment(ref lv.currentAge);
                Value = tv;
            }
        }
        /// <summary>
        /// 调整
        /// </summary>
        /// <param name="key"></param>
        public void Adjust(string key)
        {
            while (cache.Count >= maxSize)
            {
                long ageToDelete = Interlocked.Increment(ref ageToDiscard);
                var toDiscard =
                      cache.FirstOrDefault(p => p.Value.Age == ageToDelete);
                if (toDiscard.Key == null)
                    continue;
                TrackValue old;
                cache.TryRemove(toDiscard.Key, out old);
            }
        }

        public TValue Get(string key)
        {
            try
            {
                TrackValue value = null;
                if (cache.TryGetValue(key, out value))
                {
                    value.Age = Interlocked.Increment(ref currentAge);
                }
                return value.Value;
            }
            catch (Exception ex)
            {

                return default(TValue);
            }
           
        }
        /*****************************************************/
        public Tuple<TrackValue, bool> CheckExpire(string key)
        {
            TrackValue result;
            if (cache.TryGetValue(key, out result))
            {
                var age = DateTime.Now.Subtract(result.CreateTime);
                if (age >= maxTime || age >= result.ExpireTime)
                {
                    TrackValue old;
                    cache.TryRemove(key, out old);
                    return Tuple.Create(default(TrackValue), false);
                }
            }
            return Tuple.Create(result, true);
        }
        /// <summary>
        /// 检查缓存是否过期,过期则删除 需要通过定时任务或者线程调用
        /// </summary>
        public void Inspection()
        {
            foreach (var item in this)
            {
                CheckExpire(item.Key);
            }
        }
    }
