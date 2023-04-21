namespace Obertonizer
{
    public class Pipeline
    {
        public void Clear()
        {
            _items.Clear();
        }

        public void AddItem(IPipelineItem item)
        {
            if (_items.Count > 0)
            {
                var last = _items[_items.Count - 1];
                if (last.OutputType != item.InputType)
                {
                    throw new Exception("Incomptable input type: " + item.InputType.Name + " : " + last.OutputType.Name);
                }
            }
            _items.Add(item);
        }

        protected List<IPipelineItem> _items = new List<IPipelineItem>();

        public IPipelineItem[] Items
        {
            get
            {
                return _items.ToArray();
            }
        }
        public List<TimeSpan> Spans = new List<TimeSpan>();

        public List<object> Results = new List<object>();

        public object Process(object input)
        {
            object o = input;
            Spans.Clear();
            Results.Clear();
            Results.Add(o);
            foreach (var item in _items)
            {

                DateTime dt = DateTime.Now;
                if (item.Enabled)
                {
                    o = item.Process(o);
                }
                Results.Add(o);
                Spans.Add(DateTime.Now.Subtract(dt));
            }
            return o;
        }
    }
}