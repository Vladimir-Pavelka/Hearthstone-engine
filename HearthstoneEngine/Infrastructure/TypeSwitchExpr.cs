namespace Infrastructure
{
    using System;

    public static class TypeSwitchExpr
    {
        public static SwitchExpr<TSource, TResult> On<TSource, TResult>(TSource value) where TSource : class
        {
            return new SwitchExpr<TSource, TResult>(value);
        }

        public class SwitchExpr<TSource, TResult> where TSource : class
        {
            private readonly TSource _value;

            private TResult _result;

            private bool _handled;

            internal SwitchExpr(TSource value)
            {
                _value = value;
            }

            public SwitchExpr<TSource, TResult> Case<TTarget>(Func<TResult> action)
                where TTarget : TSource
            {
                return Case<TTarget>(null, action);
            }

            public SwitchExpr<TSource, TResult> Case<TTarget>(Func<TTarget, bool> condition, Func<TResult> action)
              where TTarget : TSource
            {
                if (action == null) throw new ArgumentException("action must not be null");

                if (_handled || _value == null) return this;
                var sourceType = _value.GetType();
                var targetType = typeof(TTarget);
                if (!targetType.IsAssignableFrom(sourceType)) return this;
                if (condition != null && !condition((TTarget)_value)) return this;
                _result = action();
                _handled = true;

                return this;
            }

            public SwitchExpr<TSource, TResult> Case<TTarget>(Func<TTarget, bool> condition, Func<TTarget, TResult> action)
              where TTarget : TSource
            {
                if (action == null) throw new ArgumentException("action must not be null");

                if (_handled || _value == null) return this;
                var sourceType = _value.GetType();
                var targetType = typeof(TTarget);
                if (!targetType.IsAssignableFrom(sourceType)) return this;
                if (condition != null && !condition((TTarget)_value)) return this;
                _result = action((TTarget)_value);
                _handled = true;

                return this;
            }

            public SwitchExpr<TSource, TResult> Case<TTarget>(Func<TTarget, TResult> action)
                where TTarget : TSource
            {
                if (action == null) throw new ArgumentException("action must not be null");

                if (_handled || _value == null) return this;
                var sourceType = _value.GetType();
                var targetType = typeof(TTarget);
                if (!targetType.IsAssignableFrom(sourceType)) return this;
                _result = action((TTarget)_value);
                _handled = true;

                return this;
            }

            public TResult Default(Func<TResult> action)
            {
                if (action == null) throw new ArgumentException("action must not be null");

                if (!_handled)
                {
                    _result = action();
                }

                return _result;
            }

            public TResult Default(Func<TSource, TResult> action)
            {
                if (action == null) throw new ArgumentException("action must not be null");

                if (!_handled)
                {
                    _result = action(_value);
                }

                return _result;
            }

            public TResult ElseThrow()
            {
                if (!_handled)
                {
                    throw new ArgumentException("TypeSwitch: No case matched", _value.ToString());
                }

                return _result;
            }
        }
    }
}