using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace IMClient.Helper
{
    public partial class ActionCenter
    {
        public static RoutedCommand SureCommand = new RoutedCommand();
        public ActionCenter() { }
    }
    public class CallSetObject : TargetedTriggerAction<DependencyObject>
    {
        public static readonly DependencyProperty ValProperty = DependencyProperty.Register("Val", typeof(object), typeof(CallSetObject), null);
        public object Val
        {
            get { return (object)GetValue(ValProperty); }
            set { SetValue(ValProperty, value); }
        }
        public static readonly DependencyProperty TargetPropertyProperty = DependencyProperty.Register("TargetProperty", typeof(object), typeof(CallSetObject), null);
        public object TargetProperty
        {
            get { return (object)GetValue(TargetPropertyProperty); }
            set { SetValue(TargetPropertyProperty, value); }
        }
        protected override void Invoke(object parameter)
        {
            if (Val != null && TargetProperty != null)
            {
                Target.SetValue(TargetPropertyProperty, Val);
            }
        }
    }

    public class CallInvokeMethod2 : TargetedTriggerAction<DependencyObject>
    {
        public static readonly DependencyProperty DestTypeProperty = DependencyProperty.Register("DestType", typeof(Type), typeof(CallInvokeMethod2), new PropertyMetadata(null));
        public Type DestType
        {
            get { return (Type)GetValue(DestTypeProperty); }
            set { SetValue(DestTypeProperty, value); }
        }

        public static readonly DependencyProperty ParasObjectProperty = DependencyProperty.Register("ParasObject", typeof(object), typeof(CallInvokeMethod2), new PropertyMetadata(null));
        public object ParasObject
        {
            get { return (object)GetValue(ParasObjectProperty); }
            set { SetValue(ParasObjectProperty, value); }
        }

        public static readonly DependencyProperty DestParasProperty = DependencyProperty.Register("DestParas", typeof(object), typeof(CallInvokeMethod2), new PropertyMetadata(null));
        public object DestParas
        {
            get { return (object)GetValue(DestParasProperty); }
            set { SetValue(DestParasProperty, value); }
        }

        public static readonly DependencyProperty DestMethodProperty = DependencyProperty.Register("DestMethod", typeof(string), typeof(CallInvokeMethod2), new PropertyMetadata(null));
        public string DestMethod
        {
            get { return (string)GetValue(DestMethodProperty); }
            set { SetValue(DestMethodProperty, value); }
        }

        public static readonly DependencyProperty DestObjectProperty = DependencyProperty.Register("DestObject", typeof(object), typeof(CallInvokeMethod2), new PropertyMetadata(null));
        public object DestObject
        {
            get { return (object)GetValue(DestObjectProperty); }
            set { SetValue(DestObjectProperty, value); }
        }

        public static readonly DependencyProperty TargetPropertyProperty = DependencyProperty.Register("TargetProperty", typeof(DependencyProperty), typeof(CallInvokeMethod2), new PropertyMetadata(null));
        public DependencyProperty TargetProperty
        {
            get { return (DependencyProperty)GetValue(TargetPropertyProperty); }
            set { SetValue(TargetPropertyProperty, value); }
        }

        public static readonly DependencyProperty IsDirectProperty = DependencyProperty.Register("IsDirect", typeof(bool), typeof(CallInvokeMethod2), new PropertyMetadata(false));
        public bool IsDirect
        {
            get { return (bool)GetValue(IsDirectProperty); }
            set { SetValue(IsDirectProperty, value); }
        }
        protected override void Invoke(object parameter)
        {
            if (DestType != null && !string.IsNullOrEmpty(DestMethod))
            {
                MethodInfo mInfo = DestType.GetMethod(DestMethod, BindingFlags.Public | BindingFlags.Static);
                if (mInfo != null)
                {
                    if (IsDirect)
                    {
                        object o1 = mInfo.Invoke(DestObject, (object[])DestParas);
                        if (Target != null && TargetProperty != null)
                            Target.SetValue(TargetProperty, o1);
                    }
                    else
                    {
                        List<object> l1 = new List<object>();
                        switch (parameter.GetType().FullName)
                        {
                            case "System.Windows.Controls.CalendarSelectionChangedEventArgs":
                                var c1 = parameter as System.Windows.Controls.SelectionChangedEventArgs;
                                string date = c1.AddedItems.Count > 0 ? ((DateTime)c1.AddedItems[0]).ToString("yyyy-MM-dd") : "";
                                l1.Add(date);
                                l1.Add(this.ParasObject);
                                l1.Add(this.DestParas);
                                break;
                            case "System.Windows.Controls.SelectionChangedEventArgs":
                                string date2 = this.ParasObject != null ? (DateTime.Parse(this.ParasObject.ToString())).ToString("yyyy-MM-dd") : "";
                                l1.Add(date2);
                                l1.Add(this.ParasObject);
                                l1.Add(this.DestParas);
                                break;
                            case "System.Windows.RoutedEventArgs":
                            case "System.Windows.Controls.TextChangedEventArgs":
                                l1.Add(this.ParasObject);
                                l1.Add(this.DestParas);
                                break;
                            default: break;
                        }
                        object o1 = mInfo.Invoke(DestObject, l1.ToArray());
                        if (Target != null && TargetProperty != null)
                            Target.SetValue(TargetProperty, o1);
                    }
                }
            }
        }
    }

    public class CallSetObjectProperty : TargetedTriggerAction<object>
    {
        public object Val
        {
            get { return (object)GetValue(ValProperty); }
            set { SetValue(ValProperty, value); }
        }
        public static readonly DependencyProperty ValProperty = DependencyProperty.Register("Val", typeof(object), typeof(CallSetObjectProperty), new UIPropertyMetadata());
        protected override void Invoke(object parameter)
        {
            if (TargetObject != null && TargetName != null)
            {
                Type t1 = TargetObject.GetType();
                PropertyInfo pInfo = t1.GetProperty(TargetName);
                if (pInfo != null && this.Val != null)
                {
                    pInfo.SetValue(TargetObject, this.Val.ToString(), null);
                }
            }
        }
    }

    public class CallInvokeMethod : TargetedTriggerAction<DependencyObject>
    {
        public Type DestType
        {
            get { return (Type)GetValue(DestTypeProperty); }
            set { SetValue(DestTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DestType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DestTypeProperty =
            DependencyProperty.Register("DestType", typeof(Type), typeof(CallInvokeMethod), new PropertyMetadata(null));

        public object ParasObject
        {
            get { return (object)GetValue(ParasObjectProperty); }
            set { SetValue(ParasObjectProperty, value); }
        }

        public static readonly DependencyProperty ParasObjectProperty = DependencyProperty.Register("ParasObject", typeof(object), typeof(CallInvokeMethod), new PropertyMetadata(null));

        public object DestParas
        {
            get { return (object)GetValue(DestParasProperty); }
            set { SetValue(DestParasProperty, value); }
        }

        public static readonly DependencyProperty DestParasProperty = DependencyProperty.Register("DestParas", typeof(object), typeof(CallInvokeMethod), new PropertyMetadata(null));
        public string DestMethod
        {
            get { return (string)GetValue(DestMethodProperty); }
            set { SetValue(DestMethodProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DestMethod.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DestMethodProperty =
            DependencyProperty.Register("DestMethod", typeof(string), typeof(CallInvokeMethod), new UIPropertyMetadata());

        public object DestObject
        {
            get { return (object)GetValue(DestObjectProperty); }
            set { SetValue(DestObjectProperty, value); }
        }
        public static readonly DependencyProperty DestObjectProperty = DependencyProperty.Register("DestObject", typeof(object), typeof(CallInvokeMethod), new PropertyMetadata(null));


        public DependencyProperty TargetProperty
        {
            get { return (DependencyProperty)GetValue(DestObjectProperty); }
            set { SetValue(DestObjectProperty, value); }
        }
        public static readonly DependencyProperty TargetPropertyProperty = DependencyProperty.Register("TargetProperty", typeof(DependencyProperty), typeof(CallInvokeMethod), new PropertyMetadata(null));

        public bool IsDirect
        {
            get { return (bool)GetValue(DestObjectProperty); }
            set { SetValue(DestObjectProperty, value); }
        }
        public static readonly DependencyProperty IsDirectProperty = DependencyProperty.Register("IsDirect", typeof(bool), typeof(CallInvokeMethod), new PropertyMetadata(null));

        protected override void Invoke(object parameter)
        {
            if (DestType != null && !string.IsNullOrEmpty(DestMethod))
            {
                MethodInfo mInfo = DestType.GetMethod(DestMethod, BindingFlags.Public | BindingFlags.Static);
                if (mInfo != null)
                {
                    if (IsDirect)
                    {
                        object o1 = mInfo.Invoke(DestObject, (object[])DestParas);
                        if (Target != null && TargetProperty != null)
                            Target.SetValue(TargetProperty, o1);

                    }
                    else
                    {
                        List<object> l1 = new List<object>();
                        switch (parameter.GetType().FullName)
                        {
                            case "System.Windows.Controls.CalendarSelectionChangedEventArgs":
                                var c1 = parameter as System.Windows.Controls.SelectionChangedEventArgs;
                                string date = c1.AddedItems.Count > 0 ? ((DateTime)c1.AddedItems[0]).ToString("yyyy-MM-dd") : "";
                                l1.Add(date);
                                l1.Add(this.ParasObject);
                                l1.Add(this.DestParas);
                                break;
                            case "System.Windows.Controls.SelectedCellsChangedEventArgs":
                                string date2 = this.ParasObject != null ? (DateTime.Parse(this.ParasObject.ToString())).ToString("yyyy-MM-dd") : "";
                                l1.Add(date2);
                                l1.Add(this.ParasObject);
                                l1.Add(this.DestParas);
                                break;
                            default:
                                break;
                        }
                        object o1 = mInfo.Invoke(DestObject, l1.ToArray());
                        if (Target != null && TargetProperty != null)
                            Target.SetValue(TargetProperty, o1);
                    }
                }
            }
        }
    }

    public class CallInvokeObjectMethod : TargetedTriggerAction<object>
    {
        public CallInvokeObjectMethod()
            : base()
        {
            this.ParameterTypes = new List<string>();
        }
        public Type DestType
        {
            get { return (Type)GetValue(DestTypeProperty); }
            set { SetValue(DestTypeProperty, value); }
        }

        public static readonly DependencyProperty DestTypeProperty = DependencyProperty.Register("DestType", typeof(Type), typeof(CallInvokeObjectMethod), new UIPropertyMetadata());

        public string DestMethod
        {
            get { return (string)GetValue(DestMethodProperty); }
            set { SetValue(DestMethodProperty, value); }
        }
        public static readonly DependencyProperty DestMethodProperty = DependencyProperty.Register("DestMethod", typeof(string), typeof(CallInvokeObjectMethod), new UIPropertyMetadata(""));

        public object MethodParameters
        {
            get { return (object)GetValue(MethodParametersProperty); }
            set { SetValue(MethodParametersProperty, value); }
        }
        public static readonly DependencyProperty MethodParametersProperty = DependencyProperty.Register("MethodParameters", typeof(object[]), typeof(CallInvokeObjectMethod), new UIPropertyMetadata());

        public List<string> ParameterTypes
        {
            get { return (List<string>)GetValue(ParameterTypesProperty); }
            set { SetValue(ParameterTypesProperty, value); }
        }
        public static readonly DependencyProperty ParameterTypesProperty = DependencyProperty.Register("ParameterTypes", typeof(List<string>), typeof(CallInvokeObjectMethod), new UIPropertyMetadata());

        protected override void Invoke(object parameter)
        {
            if (DestType != null && !string.IsNullOrEmpty(DestMethod))
            {
                MethodInfo mInfo = null;
                if (ParameterTypes.Count > 0)
                {
                    var ts = from s1 in this.ParameterTypes select Type.GetType(s1);
                    mInfo = DestType.GetMethod(DestMethod, ts.ToArray());
                }
                else
                    mInfo = DestType.GetMethod(DestMethod);
                if (mInfo != null)
                {
                    object o1 = mInfo.Invoke(TargetObject, (object[])MethodParameters);
                }

            }
        }

    }
    class EvalHelper : DependencyObject
    {
        public object Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(EvalHelper));
    }

    public class EventHandel : Behavior<UIElement>
    {
        public ScrollViewer TargetElement
        {
            get { return (ScrollViewer)GetValue(TargetElementProperty); }
            set { SetValue(TargetElementProperty, value); }
        }
        public static readonly DependencyProperty TargetElementProperty = DependencyProperty.Register("TargetElement", typeof(ScrollViewer), typeof(EventHandel));

        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.PreviewMouseWheel += AssociatedObject_PreviewMouseWheel;
        }
        private void AssociatedObject_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (this.TargetElement != null)
                this.TargetElement.ScrollToVerticalOffset(this.TargetElement.VerticalOffset - e.Delta);
        }
    }
}
