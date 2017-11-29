# LRUCache

#### What is it?

轻量级基于线程安全的LRU算法 for .net

#### How can I get it?

下载LRUCache.cs,添加到工程即可;

#### Example Usage
````C#
private static LRUCache<long> cacheScanQty = new LRUCache<long>(100, new TimeSpan(0, 10, 0));
 string qtyKey = "ScanQty" + orderCode;
            //先读取
            var qty = cacheScanQty.Get(qtyKey);
            if (qty != null && qty >0 )//cache 存在
            {
                lock (this)
                {
                    qty = qty + ScanCount ;
                    cacheScanQty.Set(qtyKey, qty);
                    Console.WriteLine(String.Format("读Cache"));

                    return qty;
                }
            }
            else
            {
                Console.WriteLine(String.Format("读数据库"));

                long count = 1;
                cacheScanQty.Set(qtyKey, count);//缓存LRU
                return count;
            }


