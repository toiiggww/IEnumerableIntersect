using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace IEnumerableIntersect
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new TestVM() { Dispatcher = this.Dispatcher };
        }
    }

    public class TestVM : ViewModelBase
    {
        public TestVM()
        {
            mbrComparer.OnComparer += (o, e) => LogsResultAdd(e);
            mbrSelfComparer.OnComparer += (o, e) => LogsDefaultAdd(e);
            mbrTimer = new Timer(x => Dispatcher?.Invoke(() => trySwitchEventOrder()), null, Timeout.Infinite, Timeout.Infinite);
        }

        private void LogsResultAdd(ComparerEvent<TestObject<string>> e)
        {
            e.Index = ComparerLogObjectComparer.Count;
            e.Type = "rst";
            ComparerLogObjectComparer.Add(e);
            mbrTimer.Change(3000, Timeout.Infinite);
        }

        public ObservableCollection<TestObject<string>> Source { get; private set; } = new ObservableCollection<TestObject<string>>();
        public ObservableCollection<TestObject<string>> ComparerByCompareResult { get; private set; } = new ObservableCollection<TestObject<string>>();
        public ObservableCollection<TestObject<string>> CompareByObjectResult { get; private set; } = new ObservableCollection<TestObject<string>>();
        public ObservableCollection<TestObject<string>> Dest { get; private set; } = new ObservableCollection<TestObject<string>>();
        public ObservableCollection<ComparerEvent<TestObject<string>>> ComparerLogObjectComparer { get; private set; } = new ObservableCollection<ComparerEvent<TestObject<string>>>();
        public ObservableCollection<ComparerEvent<TestObject<string>>> ComparerLogTestObject { get; private set; } = new ObservableCollection<ComparerEvent<TestObject<string>>>();
        private List<ComparerEvent<TestObject<string>>> mbrEventCompare = new List<ComparerEvent<TestObject<string>>>(),
            mbrEventTestobj = new List<ComparerEvent<TestObject<string>>>();
        private List<TestObject<string>> mbrObjectHolder = new List<TestObject<string>>();
        private RelayCommand mNewCollection, mSwitchEventLog, mOrderEventLog;
        private Random mRandomer = new Random(DateTime.Now.Millisecond);
        private string mbrStringTempleate = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private TestObjectComparer<string> mbrComparer = new TestObjectComparer<string>();
        private TestObject<string> mbrSelfComparer = new TestObject<string>();
        private Timer mbrTimer;
        private int mbrOrderCount = 0;
        private Visibility mbrMainViewVisiblity, mbrLogVisibility = Visibility.Collapsed;

        public int Count { get; set; } = 10;
        public Dispatcher Dispatcher { get; set; }
        public Visibility Mainvisi
        {
            get { return mbrMainViewVisiblity; }
            private set
            {
                if (value == mbrMainViewVisiblity)
                {
                    return;
                }
                mbrMainViewVisiblity = value;
                RaisePropertyChanged(nameof(Mainvisi));
            }
        }
        public Visibility Logvisi
        {
            get { return mbrLogVisibility; }
            private set
            {
                if (value == mbrLogVisibility)
                {
                    return;
                }
                mbrLogVisibility = value;
                RaisePropertyChanged(nameof(Logvisi));
            }
        }

        public RelayCommand Gen
        {
            get
            {
                return mNewCollection ?? (mNewCollection = new RelayCommand(() =>
                {
                    Source.Clear();
                    Dest.Clear();
                    CompareByObjectResult.Clear();
                    ComparerByCompareResult.Clear();

                    ComparerLogObjectComparer.Clear();
                    ComparerLogTestObject.Clear();

                    int l = mbrStringTempleate.Length;
                    mbrObjectHolder.Clear();
                    for (int i = 0; i < Count; i++)
                    {
                        TestObject<string> o = new TestObject<string>() { Value = mbrStringTempleate.Substring(mRandomer.Next() % (l - 3), 3), Int = mRandomer.Next() % 20, Index = i };
                        o.Char = o.Value[mRandomer.Next() % 3];
                        mbrObjectHolder.Add(o);
                    }
                    foreach (TestObject<string> i in mbrObjectHolder.OrderBy(x => x.Value))
                    {
                        Source.Add(i);
                    }
                    mbrObjectHolder.Clear();
                    int j = l / 2;
                    int k = mRandomer.Next() % j;
                    for (int i = 0; i < Count; i++)
                    {
                        TestObject<string> o = new TestObject<string>() { Value = mbrStringTempleate.Substring(mRandomer.Next() % (j - 3) + k, 3), Int = mRandomer.Next() % 20, Index = i };
                        o.Char = o.Value[mRandomer.Next() % 3];
                        mbrObjectHolder.Add(o);
                    }
                    foreach (TestObject<string> i in mbrObjectHolder.OrderBy(x => x.Value))
                    {
                        Dest.Add(i);
                    }
                    mbrObjectHolder.Clear();
                    foreach (TestObject<string> i in Source.Intersect(Dest).Distinct().OrderBy(x => x.Value))
                    {
                        CompareByObjectResult.Add(i);
                    }
                    foreach (TestObject<string> i in Source.Intersect(Dest, mbrComparer).Distinct(mbrComparer).OrderBy(x => x.Value))
                    {
                        ComparerByCompareResult.Add(i);
                    }
                }));
            }
        }

        private void LogsDefaultAdd(ComparerEvent<TestObject<string>> e)
        {
            e.Type = "dft";
            e.Index = ComparerLogTestObject.Count;
            ComparerLogTestObject.Add(e);
            mbrTimer.Change(3000, Timeout.Infinite);
        }

        public RelayCommand Switch
        {
            get
            {
                return mSwitchEventLog ?? (mSwitchEventLog = new RelayCommand(() =>
                {
                    if (mbrLogVisibility == Visibility.Collapsed)
                    {
                        Logvisi = Visibility.Visible;
                        Mainvisi = Visibility.Collapsed;
                    }
                    else
                    {
                        Mainvisi = Visibility.Visible;
                        Logvisi = Visibility.Collapsed;
                    }
                    mbrOrderCount = 0;
                }));
            }
        }

        public RelayCommand SwitchOrder
        {
            get
            {
                return mOrderEventLog ?? (mOrderEventLog = new RelayCommand(() => trySwitchEventOrder()));
            }
        }

        private void trySwitchEventOrder()
        {
            mbrOrderCount++;
            if (mbrMainViewVisiblity == Visibility.Visible)
            {
                int t = mbrOrderCount % 4;
                List<TestObject<string>> lstSource = null, lstDst = null, lstTest = null, lstComp = null;
                if (t == 0)
                {
                    lstSource = Source.OrderBy(x => x.Index).ToList();
                    lstDst = Dest.OrderBy(x => x.Index).ToList();
                    lstTest = CompareByObjectResult.OrderBy(x => x.Index).ToList();
                    lstComp = ComparerByCompareResult.OrderBy(x => x.Index).ToList();
                }
                else if (t == 1)
                {
                    lstSource = Source.OrderBy(x => x.Value).ToList();
                    lstDst = Dest.OrderBy(x => x.Value).ToList();
                    lstTest = CompareByObjectResult.OrderBy(x => x.Value).ToList();
                    lstComp = ComparerByCompareResult.OrderBy(x => x.Value).ToList();
                }
                else if (t == 2)
                {
                    lstSource = Source.OrderBy(x => x.Char).ToList();
                    lstDst = Dest.OrderBy(x => x.Char).ToList();
                    lstTest = CompareByObjectResult.OrderBy(x => x.Char).ToList();
                    lstComp = ComparerByCompareResult.OrderBy(x => x.Char).ToList();
                }
                else
                {
                    lstSource = Source.OrderBy(x => x.Int).ToList();
                    lstDst = Dest.OrderBy(x => x.Int).ToList();
                    lstTest = CompareByObjectResult.OrderBy(x => x.Int).ToList();
                    lstComp = ComparerByCompareResult.OrderBy(x => x.Int).ToList();
                }
                sort(lstSource, Source);
                sort(lstDst, Dest);
                sort(lstTest, CompareByObjectResult);
                sort(lstComp, ComparerByCompareResult);
            }
            else
            {
                int t = mbrOrderCount % 3;
                List<ComparerEvent<TestObject<string>>> ordDefault = null, ordCompare = null;
                if (t == 0)
                {
                    ordDefault = ComparerLogTestObject.OrderBy(x => x.Index).ToList();
                    ordCompare = ComparerLogObjectComparer.OrderBy(x => x.Index).ToList();
                }
                else if (t == 1)
                {
                    ordDefault = ComparerLogTestObject.OrderBy(x => x.X.Index).ToList();
                    ordCompare = ComparerLogObjectComparer.OrderBy(x => x.X.Index).ToList();
                }
                else
                {
                    ordDefault = ComparerLogTestObject.OrderBy(x => x.Y.Index).ToList();
                    ordCompare = ComparerLogObjectComparer.OrderBy(x => x.Y.Index).ToList();
                }
                ComparerLogObjectComparer.Clear();
                ComparerLogTestObject.Clear();
                foreach (var i in ordDefault)
                {
                    ComparerLogTestObject.Add(i);
                }
                foreach (var i in ordCompare)
                {
                    ComparerLogObjectComparer.Add(i);
                }
            }
        }

        private void sort(List<TestObject<string>> lst, ObservableCollection<TestObject<string>> view)
        {
            view.Clear();
            foreach (var i in lst)
            {
                view.Add(i);
            }
        }
    }
    public class TestObject<T> : IEqualityComparer<TestObject<T>>, IComparable
    {
        public int Int { get; set; }
        public char Char { get; set; }
        public T Value { get; set; }
        public int Index { get; set; }
        public event EventHandler<ComparerEvent<TestObject<T>>> OnComparer;

        public int CompareTo(object obj)
        {
            return (obj is TestObject<T>) ? (Equals(this, obj as TestObject<T>) ? 0 : 1) : -1;
        }

        public bool Equals(TestObject<T> x, TestObject<T> y)
        {
            OnComparer?.Invoke(this, new ComparerEvent<TestObject<T>>() { X = x, Y = y });
            return x != null & y != null ?
                x.Value.Equals(y.Value) &
                x.Int == y.Int &
                x.Char == y.Char : x != null & y != null;
        }

        public int GetHashCode(TestObject<T> obj)
        {
            return obj.Value.GetHashCode();
        }

        public override string ToString()
        {
            return $"Index : {Index,-4},V : {Value,6}, C : {Char,6}, I : {Int,6}";
        }
    }
    public class TestObjectComparer<T> : IEqualityComparer<TestObject<T>>
    {
        public event EventHandler<ComparerEvent<TestObject<T>>> OnComparer;
        public bool Equals(TestObject<T> x, TestObject<T> y)
        {
            OnComparer?.Invoke(this, new ComparerEvent<TestObject<T>>() { X = x, Y = y });
            return x != null & y != null ?
                x.Value.Equals(y.Value) &
                x.Char == y.Char : x != null & y != null;
        }

        public int GetHashCode(TestObject<T> obj)
        {
            return obj.Value.GetHashCode();
        }
    }
    public class ComparerEvent<T> : EventArgs
    {
        public string Type { get; set; }
        public int Index { get; set; }
        public T X { get; set; }
        public T Y { get; set; }
        public override string ToString()
        {
            return $"type : {Type,6}, index : {Index,6}, x : [{X}], y : [{Y}]";
        }
    }
}
