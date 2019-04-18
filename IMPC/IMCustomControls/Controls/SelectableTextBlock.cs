using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Documents;
using System.Windows.Threading;
using IMModels;

namespace IMCustomControls
{
    public class SelectableTextBlock : TextBlock
    {
        TextPointer startpoz;
        TextPointer endpoz;
        MenuItem copyMenu;
        MenuItem selectAllMenu;
        MenuItem withDrawMenu;
        MenuItem fowardMenu;

        FocusAdorner _focusAdorner;
        SelectedRectAdorner _selectedAdorner;

        public event Action<SelectableTextBlock> Deleted;
        public event Action<SelectableTextBlock> WithDraw;
        public event Action<SelectableTextBlock> FowardEvent;

        private TextRange _selection;
        public TextRange Selection
        {
            get { return _selection; }
            set
            {
                _selection = value;
                if (_selection == null || string.IsNullOrEmpty(_selection.Text))
                {
                    if (this._focusAdorner != null)
                    {
                        this._focusAdorner.Visibility = Visibility.Collapsed;
                    }

                    if (_selectedAdorner != null)
                    {
                        _selectedAdorner.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    var rectFrom = _selection.Start.GetCharacterRect(LogicalDirection.Backward);
                    var rectTo = _selection.End.GetCharacterRect(LogicalDirection.Forward);

                    double yFrom = rectFrom.Y + (rectFrom.Height - this.LineHeight) * 0.5;
                    double yTo = rectTo.Y + (rectTo.Height - this.LineHeight) * 0.5 + this.LineHeight;

                    Point pFrom = new Point(rectFrom.Location.X, yFrom);
                    Point pTo = new Point(rectTo.X + rectTo.Width, yTo);

                    SetSelectedAdorner(pFrom, pTo);
                }

            }

        }
        public bool HasSelection
        {
            get { return Selection != null && !Selection.IsEmpty; }
        }

        private void SetSelectedAdorner(Point pFrom, Point pTo)
        {
            Geometry geo;
            double h = pTo.Y - pFrom.Y;
            if (h - 6 < this.LineHeight) //单行   □■■■□
            {
                geo = new RectangleGeometry(new Rect(pFrom, pTo));
            }
            else//跨行
            {
                Point p0 = pFrom;
                Point p1 = new Point(this.ActualWidth, pFrom.Y);
                Point p2 = new Point(this.ActualWidth, pTo.Y - this.LineHeight);
                Point p3 = new Point(pTo.X, pTo.Y - this.LineHeight);
                Point p4 = pTo;
                Point p5 = new Point(0, pTo.Y);
                Point p6 = new Point(0, pFrom.Y + this.LineHeight);
                Point p7 = new Point(pFrom.X, pFrom.Y + this.LineHeight);

                // □□■■■
                // ■■■■■
                // ■■■■□
                if (pFrom.X <= pTo.X) //起点在终点左侧     
                {
                    geo = Geometry.Parse(string.Format("M{0} {1} {2} {3} {4} {5} {6} {7}z", p0, p1, p2, p3, p4, p5, p6, p7));
                }
                else
                // □□□■■
                // ■■□□□
                // □□□□□
                if (h - 6 < this.LineHeight * 2) //只跨两行，连头断开
                {
                    p3 = new Point(pFrom.X, pFrom.Y + this.LineHeight);
                    p7 = new Point(pTo.X, pFrom.Y + this.LineHeight);
                    geo = Geometry.Parse(string.Format("M{0} {1} {2} {3} z  M{4} {5} {6} {7}z", p0, p1, p2, p3, p4, p5, p6, p7));
                }
                // □□□■■
                // ■■■■■
                // ■■□□□
                else //三行以上，链接
                {
                    geo = Geometry.Parse(string.Format("M{0} {1} {2} {3} {4} {5} {6} {7}z", p0, p1, p2, p3, p4, p5, p6, p7));
                }

            }
            if (_selectedAdorner == null)
            {
                var adorLayer = AdornerLayer.GetAdornerLayer(this);
                _selectedAdorner = new SelectedRectAdorner(this, this.SelectionBrush, geo);
                adorLayer.Add(_selectedAdorner);
            }
            else
            {
                _selectedAdorner.Geometry = geo;
                _selectedAdorner.Visibility = Visibility.Visible;

            }
        }

        #region SelectionBrush

        public static readonly DependencyProperty SelectionBrushProperty =
            DependencyProperty.Register("SelectionBrush", typeof(Brush), typeof(SelectableTextBlock),
                new FrameworkPropertyMetadata(new SolidColorBrush(Colors.DarkBlue) { Opacity = 0.25 }));

        public Brush SelectionBrush
        {
            get { return (Brush)GetValue(SelectionBrushProperty); }
            set { SetValue(SelectionBrushProperty, value); }
        }

        #endregion

        public SelectableTextBlock()
        {
            this.Cursor = Cursors.IBeam;
            Focusable = true;
            var contextMenu = new ContextMenu();
            ContextMenu = contextMenu;

            copyMenu = new MenuItem();
            copyMenu.Header = "复制";
            copyMenu.InputGestureText = "Ctrl + C";
            copyMenu.Click += (ss, ee) =>
            {
                Copy();
            };
            contextMenu.Items.Add(copyMenu);

            selectAllMenu = new MenuItem();
            selectAllMenu.Header = "全选";
            selectAllMenu.InputGestureText = "Ctrl + A";
            selectAllMenu.Click += (ss, ee) =>
            {
                SelectAll();
            };
            contextMenu.Items.Add(selectAllMenu);

            fowardMenu = new MenuItem();
            fowardMenu.Header = "转发";
            fowardMenu.Click += (ss, ee) =>
            {
                FowardEvent?.Invoke(this);
            };
#if !CUSTOMSERVER
            contextMenu.Items.Add(fowardMenu);
#endif
            var deleteItem = new MenuItem();
            deleteItem.Header = "删除";
            deleteItem.Click += (ss, ee) =>
            {
                Deleted?.Invoke(this);
            };
            contextMenu.Items.Add(deleteItem);

            withDrawMenu = new MenuItem();
            withDrawMenu.Header = "撤回";
            withDrawMenu.Click += (ss, ee) =>
            {
                WithDraw?.Invoke(this);
            };



            ContextMenuOpening += contextMenu_ContextMenuOpening;

            this.LineHeight = 30;
        }

        public IEnumerable<Inline> Content
        {
            get { return (IEnumerable<Inline>)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof(IEnumerable<Inline>), typeof(SelectableTextBlock), new PropertyMetadata(OnContentPropertyChanged));

        private static void OnContentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SelectableTextBlock target = d as SelectableTextBlock;
            target.LoadNewContent();
        }

        private void LoadNewContent()
        {
            this.Inlines.Clear();

            //var datas = this.Content.ToList();
            //for (int i = 0; i < datas.Count; i++)
            //{
            //    if(datas[i] is Hyperlink hl)
            //    {
            //        if(hl.Inlines.First() is Run run)
            //        {
            //            run.ForceCursor = hl.ForceCursor;
            //            run.Cursor = hl.Cursor;
            //            run.BaselineAlignment = hl.BaselineAlignment; 
            //            run.Foreground = hl.Foreground;
            //            run.TextDecorations = hl.TextDecorations;
            //            datas[i] = run; 
            //        } 
            //    }
            //}
            this.Inlines.AddRange(Content);
        }

        void contextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            copyMenu.IsEnabled = HasSelection;

            if (this.DataContext is MessageModel msg)
            {

                if ((DateTime.Now - msg.SendTime).TotalMinutes >= 2 || !msg.IsMine || msg.MessageState == MessageStates.Loading || msg.MessageState == MessageStates.Fail || msg.MessageState == MessageStates.Warn)
                {
                    if (this.ContextMenu.Items.Contains(this.withDrawMenu))
                    {
                        this.ContextMenu.Items.Remove(this.withDrawMenu);
                    }
                }
                else
                {
                    if (!this.ContextMenu.Items.Contains(this.withDrawMenu))
                    {
                        this.ContextMenu.Items.Add(this.withDrawMenu);
                    }
                }
                if (msg.MessageState == MessageStates.Fail
                    || msg.MessageState == MessageStates.Warn
                    || msg.MsgType == MessageType.audio
                    || msg.MsgType == MessageType.redenvelopesreceive
                    || msg.MsgType == MessageType.redenvelopessendout
                    || msg.MessageState == MessageStates.Loading)
                    fowardMenu.Visibility = Visibility.Collapsed;
                else
                    fowardMenu.Visibility = Visibility.Visible;
                if (msg.MessageState == MessageStates.Fail || msg.MessageState == MessageStates.Warn)
                    withDrawMenu.Visibility = Visibility.Collapsed;
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            ReleaseMouseCapture();
            base.OnMouseLeftButtonUp(e);
            e.Handled = true;
        }

        private void Test(Point point)
        {
            //获取TextPointer
            var poz = this.GetPositionFromPoint(point, true);

            //var nextPoz = poz.GetNextInsertionPosition(poz.LogicalDirection);
            if (poz != null)
            {
                var rect = poz.GetCharacterRect(LogicalDirection.Backward);

                //var range = new TextRange(poz, nextPoz);

                double y = rect.Y + (rect.Height - this.LineHeight) * 0.5;
                rect.Y = y + 5;
                rect.Height = this.LineHeight - 10;

                rect.Width = 1;
                if (_focusAdorner == null)
                {
                    var adorLayer = AdornerLayer.GetAdornerLayer(this);
                    _focusAdorner = new FocusAdorner(this, rect);
                    adorLayer.Add(_focusAdorner);
                }
                else
                {
                    _focusAdorner.Rect = rect;
                    _focusAdorner.Visibility = Visibility.Visible;
                }
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                this.SelectAll();
            }
            else
            {
                ClearSelection();
                Keyboard.Focus(this);
                var point = e.GetPosition(this);
                startpoz = GetPositionFromPoint(point, true);
                CaptureMouse();
                Test(e.GetPosition(this));
            }

            base.OnMouseLeftButtonDown(e);
            e.Handled = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (IsMouseCaptured)
            {
                var point = e.GetPosition(this);

                endpoz = GetPositionFromPoint(point, true);

                ClearSelection();
                if (this.ContentEnd.GetOffsetToPosition(endpoz) == -1)
                {
                    //startpoz = contents;

                    //endpoz = ContentEnd;// this.ContentEnd.GetNextContextPosition(LogicalDirection.Backward) ;
                    //Console.WriteLine(startpoz.GetOffsetToPosition(endpoz));
                }
                Selection = new TextRange(startpoz, endpoz);
                //if(!string.IsNullOrEmpty(Selection.Text))
                //{
                //    BrushConverter brushConverter = new BrushConverter();
                //    Brush brush = (Brush)brushConverter.ConvertFromString("#FFFFFF");
                //    Selection.ApplyPropertyValue(TextElement.ForegroundProperty, brush);
                //}
                //else
                //{
                //    this.SelectionBrush = new SolidColorBrush(Colors.DarkBlue);
                //}




                //Selection.ApplyPropertyValue(TextElement.BackgroundProperty, SelectionBrush);
                //Selection.ApplyPropertyValue(Inline.BackgroundProperty, SelectionBrush);
                CommandManager.InvalidateRequerySuggested();
                //SetUISelectedState();
                OnSelectionChanged(EventArgs.Empty);
            }

            base.OnMouseMove(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            ClearSelection();
            base.OnLostFocus(e);

            if (_focusAdorner != null)
            {
                _focusAdorner.Visibility = Visibility.Collapsed;
            }
            if (_selectedAdorner != null)
            {
                _selectedAdorner.Visibility = Visibility.Collapsed;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.C)
                    Copy();
                else if (e.Key == Key.A)
                    SelectAll();
            }
            else if (Keyboard.Modifiers == ModifierKeys.Shift)
            {

            }

            base.OnKeyDown(e);
        }

        public bool Copy()
        {
            if (HasSelection)
            {
                this.CopyData();
                //Clipboard.SetDataObject(Selection.Text);
                return true;
            }
            return false;
        }


#region  Do Copy

        List<Inline> copys = new List<Inline>();
        private void CopyData()
        {
            var selection = this.Selection;
            if (!selection.IsEmpty)
            {
                copys.Clear();
                StringBuilder list = new StringBuilder();
                //foreach (var inline in this.Inlines)
                //{
                //    var containsLeft = selection.Contains(inline.ContentStart);
                //    var containsRight = selection.Contains(inline.ContentEnd);

                //    if (containsLeft == containsRight)
                //    {
                //        if (containsLeft)
                //        {
                //            ProcessSelectedInline(inline, null, list);
                //        }
                //        else
                //        {
                //            if (inline is Run
                //                && inline.ContentEnd.CompareTo(selection.End) > 0
                //                && inline.ContentStart.CompareTo(selection.Start) < 0)
                //            {
                //                ProcessSelectedInline(inline, selection.Text, list);
                //            }
                //        }
                //    }
                //    else if (inline is Run)
                //    {
                //        if (containsRight)
                //        {
                //            var partialText = selection.Start.GetTextInRun(LogicalDirection.Forward);
                //            ProcessSelectedInline(inline, partialText, list);
                //        }
                //        else
                //        {
                //            var partialText = selection.End.GetTextInRun(LogicalDirection.Backward);
                //            ProcessSelectedInline(inline, partialText, list);
                //        }
                //    }
                //} 
                if (!selection.IsEmpty)
                {
                    //var items = GetInlinesFromDocument(richBox.Document.Blocks).ToList();

                    foreach (var inline in this.Inlines)
                    {
                        var containsLeft = selection.Contains(inline.ContentStart);
                        var containsRight = selection.Contains(inline.ContentEnd);

                        if (containsLeft == containsRight)
                        {
                            if (containsLeft)
                            {
                                ProcessSelectedInline(inline, null, list);
                            }
                            else
                            {
                                if (inline is Run
                                    && inline.ContentEnd.CompareTo(selection.End) > 0
                                    && inline.ContentStart.CompareTo(selection.Start) < 0)
                                {
                                    ProcessSelectedInline(inline, selection.Text, list);
                                }
                            }
                        }
                        else if (inline is Run run)
                        {
                            if (containsRight)
                            {
                                var partialText = selection.Start.GetTextInRun(LogicalDirection.Forward);
                                if (run.Text.Contains(partialText))
                                {
                                    ProcessSelectedInline(inline, partialText, list);
                                }
                            }
                            else
                            {
                                var partialText = selection.End.GetTextInRun(LogicalDirection.Backward);
                                if (run.Text.Contains(partialText))
                                {
                                    ProcessSelectedInline(inline, partialText, list);
                                }
                            }
                        }
                    }
                    //string value = $"<DIV><IMG src=\"file:///{this.ImagePath }\"></DIV> ";
                    //Helper.ClipboardHelper.CopyToClipboard(value, "");
                }


                Clipboard.SetDataObject(list.ToString());
            }
        }

        void ProcessSelectedInline(Inline inline, object partial, StringBuilder list)
        {
            if (copys.Contains(inline))
            {
                return;
            }
            copys.Add(inline);
            if (inline is Run run)
            {
                if (String.IsNullOrEmpty(run.Text) || run.Text == " ")
                {
                    return;
                }

                if (partial != null)
                {
                    if (!string.IsNullOrEmpty(partial.ToString()))
                    {
                        list.Append(partial.ToString());
                    }
                }
                else
                {
                    list.Append(run.Text);
                }
            }
            else
            {
                if (inline is InlineUIContainer container && container.Child is UIElement)
                {

                    list.Append(container.Child.Uid);
                }
            }
        }

        IEnumerable<Inline> GetInlinesFromDocument(FlowDocument doc)
        {
            foreach (var block in doc.Blocks)
            {
                foreach (var inline in GetInlinesFromBlock(block))
                    yield return inline;
            }
        }

        IEnumerable<Inline> GetInlinesFromBlock(Block block)
        {
            if (block is Paragraph)
                foreach (var inline in ((Paragraph)block).Inlines)
                {
                    foreach (var subInline in GetInlinesFromInline(inline))
                        yield return subInline;
                }
            else if (block is Section)
            {
                var sec = (Section)block;
                foreach (var b in sec.Blocks)
                    foreach (var inline in GetInlinesFromBlock(b))
                        yield return inline;
            }
        }

        IEnumerable<Inline> GetInlinesFromInline(Inline inline)
        {
            if (inline is Span)
                foreach (var subInline in ((Span)inline).Inlines)
                    yield return subInline;
            yield return inline;
        }


#endregion


        public void ClearSelection()
        {
            //var contentRange = new TextRange(ContentStart, ContentEnd);
            //contentRange.ApplyPropertyValue(TextElement.BackgroundProperty, null);

            //foreach(var ly in _slectedUIs)
            //{ 
            //    ly.Key.Remove(ly.Value);
            //}
            //_slectedUIs.Clear();
            Selection = null;


        }

        public void SelectAll()
        {
            Selection = new TextRange(ContentStart, ContentEnd);
            if (this._focusAdorner != null)
            {
                this._focusAdorner.Visibility = Visibility.Collapsed;
            }
            //Selection.ApplyPropertyValue(TextElement.BackgroundProperty, SelectionBrush);
        }

        public event EventHandler SelectionChanged;

        protected virtual void OnSelectionChanged(EventArgs e)
        {
            var handler = this.SelectionChanged;
            if (handler != null)
                handler(this, e);
        }

        public class SelectedRectAdorner : Adorner
        {
            Brush _brush;

            public SelectedRectAdorner(UIElement adornedElement, Brush fill, Geometry geo)
            : base(adornedElement)
            {
                this.IsHitTestVisible = false;
                _brush = fill;
                this.Geometry = geo;
            }

            public Geometry Geometry
            {
                get { return (Geometry)GetValue(RectProperty); }
                set { SetValue(RectProperty, value); }
            }

            // Using a DependencyProperty as the backing store for Rect.  This enables animation, styling, binding, etc...
            public static readonly DependencyProperty RectProperty =
                DependencyProperty.Register("Geometry", typeof(Geometry), typeof(SelectedRectAdorner),
                    new FrameworkPropertyMetadata(Geometry.Empty, FrameworkPropertyMetadataOptions.AffectsRender));




            protected override void OnRender(DrawingContext drawingContext)
            {
                //找出控件所围成的矩形区域   
                drawingContext.DrawGeometry(this._brush, null, this.Geometry);
            }
        }

        public class FocusAdorner : Adorner
        {
            DispatcherTimer _timer;

            public FocusAdorner(UIElement adornedElement, Rect rect)
            : base(adornedElement)
            {
                this.IsHitTestVisible = false;

                this.Rect = rect;
                _timer = new DispatcherTimer();
                _timer.Interval = TimeSpan.FromMilliseconds(500);
                _timer.Tick += _timer_Tick;
                this.IsVisibleChanged += FocusAdorner_IsVisibleChanged;
            }

            private void _timer_Tick(object sender, EventArgs e)
            {
                if (this.Stroke == Brushes.Black)
                {
                    this.Stroke = null;
                }
                else
                {
                    this.Stroke = Brushes.Black;
                }
            }

            private void FocusAdorner_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
            {
                if (this.IsVisible)
                {
                    _timer.Start();
                }
                else
                {
                    _timer.Stop();
                }
            }

            public Rect Rect
            {
                get { return (Rect)GetValue(RectProperty); }
                set { SetValue(RectProperty, value); }
            }

            // Using a DependencyProperty as the backing store for Rect.  This enables animation, styling, binding, etc...
            public static readonly DependencyProperty RectProperty =
                DependencyProperty.Register("Rect", typeof(Rect), typeof(FocusAdorner),
                    new FrameworkPropertyMetadata(Rect.Empty, FrameworkPropertyMetadataOptions.AffectsRender));



            public Brush Stroke
            {
                get { return (Brush)GetValue(StrokeProperty); }
                set { SetValue(StrokeProperty, value); }
            }

            // Using a DependencyProperty as the backing store for Stroke.  This enables animation, styling, binding, etc...
            public static readonly DependencyProperty StrokeProperty =
                DependencyProperty.Register("Stroke", typeof(Brush), typeof(FocusAdorner),
                    new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));

            protected override void OnRender(DrawingContext drawingContext)
            {
                //找出控件所围成的矩形区域 
                drawingContext.DrawRectangle(this.Stroke, null, this.Rect);
            }
        }
    }
    //public class SelectableTextBlock : TextBlock
    //{
    //    static SelectableTextBlock PERTARGET;

    //    public static void Clear()
    //    {
    //        if (PERTARGET != null)
    //        {
    //            PERTARGET.ClearSelection();
    //            PERTARGET = null;
    //        } 
    //    }


    //    TextPointer startpoz;
    //    TextPointer endpoz;
    //    MenuItem copyMenu;
    //    MenuItem selectAllMenu;

    //    public TextRange Selection { get; private set; }
    //    public bool HasSelection
    //    {
    //        get { return this.Background != null; }
    //    }

    //    public delegate void DoCopyActionHandler(SelectableTextBlock sender); 
    //    public event DoCopyActionHandler DoCopyAction;
    //    public event DoCopyActionHandler DoDeleteAction;

    //    #region SelectionBrush

    //    public static readonly DependencyProperty SelectionBrushProperty =
    //        DependencyProperty.Register("SelectionBrush", typeof(Brush), typeof(SelectableTextBlock),
    //            new FrameworkPropertyMetadata(new SolidColorBrush(Colors.DarkBlue) { Opacity = 0.25 }));

    //    public Brush SelectionBrush
    //    {
    //        get { return (Brush)GetValue(SelectionBrushProperty); }
    //        set { SetValue(SelectionBrushProperty, value); }
    //    }

    //    #endregion



    //    public IEnumerable<Inline> Content
    //    {
    //        get { return (IEnumerable<Inline>)GetValue(ContentProperty); }
    //        set { SetValue(ContentProperty, value); }
    //    }

    //    public static readonly DependencyProperty ContentProperty =
    //        DependencyProperty.Register("Content", typeof(IEnumerable<Inline>), typeof(SelectableTextBlock), new PropertyMetadata(OnContentPropertyChanged));

    //    private static void OnContentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    //    {
    //        SelectableTextBlock target = d as SelectableTextBlock;
    //        target.LoadNewContent();
    //    }

    //    private void LoadNewContent()
    //    {
    //        this.Inlines.Clear();
    //        this.Inlines.AddRange(this.Content);
    //    }

    //    public SelectableTextBlock()
    //    {
    //        this.Cursor = Cursors.IBeam;
    //        Focusable = true;
    //        var contextMenu = new ContextMenu();
    //        ContextMenu = contextMenu;

    //        copyMenu = new MenuItem();
    //        copyMenu.Header = "复制";
    //        //copyMenu.InputGestureText = "Ctrl + C";
    //        copyMenu.Click += (ss, ee) =>
    //        {
    //            Copy();
    //        };
    //        contextMenu.Items.Add(copyMenu);

    //        copyMenu = new MenuItem();
    //        copyMenu.Header = "删除";
    //        //copyMenu.InputGestureText = "Ctrl + C";
    //        copyMenu.Click += (ss, ee) =>
    //        {
    //            this.DoDeleteAction?.Invoke(this);
    //        };
    //        contextMenu.Items.Add(copyMenu);

    //        //selectAllMenu = new MenuItem();
    //        //selectAllMenu.Header = "全选";
    //        //selectAllMenu.InputGestureText = "Ctrl + A";
    //        //selectAllMenu.Click += (ss, ee) =>
    //        //{
    //        //    SelectAll();
    //        //};
    //        //contextMenu.Items.Add(selectAllMenu);

    //        ContextMenuOpening += contextMenu_ContextMenuOpening;

    //        this.Unloaded += SelectableTextBlock_Unloaded;


    //    }



    //    private void SelectableTextBlock_Unloaded(object sender, RoutedEventArgs e)
    //    {
    //        ClearSelection();
    //    }

    //    void contextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
    //    {
    //        copyMenu.IsEnabled = HasSelection;
    //    }
    //    protected override void OnMouseUp(MouseButtonEventArgs e)
    //    {
    //        ReleaseMouseCapture();
    //        base.OnMouseUp(e);
    //    }

    //    protected override void OnMouseDown(MouseButtonEventArgs e)
    //    {
    //        if (PERTARGET != null )
    //        {
    //            PERTARGET.ClearSelection();

    //            if (PERTARGET == this &&e.LeftButton==MouseButtonState.Pressed)
    //            {
    //                PERTARGET = null;
    //                return;
    //            }
    //        } 
    //        //Keyboard.Focus(this);
    //        //var point = e.GetPosition(this);
    //        //startpoz = GetPositionFromPoint(point, true);
    //        CaptureMouse();
    //        base.OnMouseDown(e);
    //        PERTARGET = this;
    //        this.SelectAll();
    //    } 
    //    //private Dictionary<Visual, SelectedAdorner> _slectedUIs = new Dictionary<Visual, SelectedAdorner>();

    //    //SelectedAdorner _selectedAdorner;

    //    protected override void OnLostFocus(RoutedEventArgs e)
    //    {
    //        ClearSelection();
    //        base.OnLostFocus(e); 
    //    }


    //    private AdornerLayer _adornerLayer;
    //    public AdornerLayer AdornerLayer
    //    {
    //        get
    //        {
    //            if (_adornerLayer == null)
    //            {
    //                _adornerLayer = AdornerLayer.GetAdornerLayer(this);
    //            }
    //            return _adornerLayer;
    //        }
    //    }

    //    protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
    //    {
    //        base.OnLostKeyboardFocus(e);
    //    }


    //    protected override void OnPreviewKeyUp(KeyEventArgs e)
    //    {
    //        if (Keyboard.Modifiers == ModifierKeys.Control)
    //        {
    //            if (e.Key == Key.C)
    //                Copy();
    //            else if (e.Key == Key.A)
    //                SelectAll();
    //        }

    //        base.OnKeyUp(e);
    //    }


    //    public bool Copy()
    //    {
    //        if (HasSelection)
    //        {
    //            Clipboard.SetDataObject(this.Inlines); 
    //            DoCopyAction?.Invoke(this);
    //            return true;
    //        }
    //        return false;
    //    }



    //    public void ClearSelection()
    //    {
    //        //if (_selectedAdorner != null)
    //        //{
    //        //    this.AdornerLayer.Remove(_selectedAdorner);
    //        //}

    //        //_selectedAdorner = null;
    //        //Selection = null;
    //        this.Background = null;
    //    }

    //    public void SelectAll()
    //    {
    //        this.Background = this.SelectionBrush;
    //        //_selectedAdorner = new SelectedAdorner(this, this.SelectionBrush);
    //        //AdornerLayer.Add(_selectedAdorner);
    //    }

    //    //public event EventHandler SelectionChanged;

    //    //protected virtual void OnSelectionChanged(EventArgs e)
    //    //{
    //    //    var handler = this.SelectionChanged;
    //    //    if (handler != null)
    //    //        handler(this, e);
    //    //}


    //    //public class SelectedAdorner : Adorner
    //    //{
    //    //    Brush _brush;
    //    //    public SelectedAdorner(UIElement adornedElement,Brush fill)
    //    //    : base(adornedElement)
    //    //    {
    //    //        _brush = fill;
    //    //        IsHitTestVisible = false;
    //    //    }
    //    //    protected override void OnRender(DrawingContext drawingContext)
    //    //    {
    //    //        //找出控件所围成的矩形区域
    //    //        Rect rect = new Rect(-1,-5, this.AdornedElement.DesiredSize.Width+2, this.AdornedElement.DesiredSize.Height+10);

    //    //        drawingContext.DrawRectangle(this._brush, null, rect);
    //    //    }
    //    //}

    //}


    public class SuperSelectableTextBlock : SelectableTextBlock
    {
        public SuperSelectableTextBlock()
        {
            ContextMenuOpening += SuperSelectableTextBlock_ContextMenuOpening;
            this.SizeChanged += SuperTextBlock_SizeChanged;

        }

        private void SuperSelectableTextBlock_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            this.ContextMenu = null;
        }

        private void SuperTextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SuperSelectableTextBlock textBlock = sender as SuperSelectableTextBlock;
            Size size = e.NewSize;
            if (size.Height > 30)
            {
                this.SizeChanged -= SuperTextBlock_SizeChanged;
                this.Height = 30;
                this.AppendVisibility = Visibility.Visible;
                this.TextTrimming = TextTrimming.WordEllipsis;
            }
            else
            {
                this.AppendVisibility = Visibility.Collapsed;
            }
        }

        public Visibility AppendVisibility
        {
            get { return (Visibility)GetValue(AppendVisibilityProperty); }
            set { SetValue(AppendVisibilityProperty, value); }
        }

        public static readonly DependencyProperty AppendVisibilityProperty =
           DependencyProperty.RegisterAttached("AppendVisibility", typeof(Visibility), typeof(SuperSelectableTextBlock), new PropertyMetadata(Visibility.Collapsed));


    }
}
