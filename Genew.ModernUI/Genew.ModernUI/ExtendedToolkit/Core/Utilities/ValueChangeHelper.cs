﻿/*******************************************************************
* 版权所有： 深圳市震有科技有限公司
* 文件名称： ValueChangeHelper.cs
* 作   者： chenzhifen
* 创建日期： 2013-12-26 23:25
* 文件版本： 1.0.0.0
* 修改时间：             修改人：                修改内容：
*******************************************************************/

using System;
using System.Collections;
using System.Windows;
using System.Windows.Data;

namespace ModernUI.ExtendedToolkit.Utilities
{
    /// <summary>
    ///     This helper class will raise events when a specific
    ///     path value on one or many items changes.
    /// </summary>
    internal class ValueChangeHelper : DependencyObject
    {
        #region Value Property

        /// <summary>
        ///     This private property serves as the target of a binding that monitors the value of the binding
        ///     of each item in the source.
        /// </summary>
        private static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object),
            typeof(ValueChangeHelper), new UIPropertyMetadata(null, OnValueChanged));

        private object Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((ValueChangeHelper)sender).RaiseValueChanged();
        }

        #endregion

        public event EventHandler ValueChanged;

        #region Constructor

        public ValueChangeHelper(Action changeCallback)
        {
            if (changeCallback == null)
                throw new ArgumentNullException("changeCallback");

            ValueChanged += (s, args) => changeCallback();
        }

        #endregion

        #region Methods

        public void UpdateValueSource(object sourceItem, string path)
        {
            BindingBase binding = null;
            if (sourceItem != null && path != null)
            {
                binding = new Binding(path) { Source = sourceItem };
            }

            UpdateBinding(binding);
        }

        public void UpdateValueSource(IEnumerable sourceItems, string path)
        {
            BindingBase binding = null;
            if (sourceItems != null && path != null)
            {
                MultiBinding multiBinding = new MultiBinding();
                multiBinding.Converter = new BlankMultiValueConverter();

                foreach (var item in sourceItems)
                {
                    multiBinding.Bindings.Add(new Binding(path) { Source = item });
                }

                binding = multiBinding;
            }

            UpdateBinding(binding);
        }

        private void UpdateBinding(BindingBase binding)
        {
            if (binding != null)
            {
                BindingOperations.SetBinding(this, ValueChangeHelper.ValueProperty, binding);
            }
            else
            {
                ClearBinding();
            }
        }

        private void ClearBinding()
        {
            BindingOperations.ClearBinding(this, ValueChangeHelper.ValueProperty);
        }

        private void RaiseValueChanged()
        {
            if (ValueChanged != null)
            {
                ValueChanged(this, EventArgs.Empty);
            }
        }

        #endregion

        #region BlankMultiValueConverter private class

        private class BlankMultiValueConverter : IMultiValueConverter
        {
            public object Convert(object[] values, Type targetType, object parameter,
                System.Globalization.CultureInfo culture)
            {
                // We will not use the result anyway. We just want the change notification to kick in.
                // Return a new object to have a different value.
                return new object();
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
                System.Globalization.CultureInfo culture)
            {
                throw new InvalidOperationException();
            }
        }

        #endregion
    }
}