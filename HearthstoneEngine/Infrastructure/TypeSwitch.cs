namespace Infrastructure
{
    using System;

    public static class TypeSwitch
    {
        public static Switch<TSource> On<TSource>(TSource value) where TSource : class
        {
            return new Switch<TSource>(value);
        }

        public class Switch<TSource> where TSource : class
        {
            private readonly TSource _value;

            private bool _handled;

            internal Switch(TSource value)
            {
                _value = value;
            }

            public Switch<TSource> Case<TTarget>(Action action)
                where TTarget : TSource
            {
                return Case<TTarget>(null, action);
            }

            public Switch<TSource> Case<TTarget>(Func<TTarget, bool> condition, Action action)
              where TTarget : TSource
            {
                if (action == null) throw new ArgumentException("action must not be null");

                if (_handled || _value == null) return this;
                var sourceType = _value.GetType();;
                var targetType = typeof(TTarget);
                if (!targetType.IsAssignableFrom(sourceType)) return this;
                if (condition != null && !condition((TTarget)_value)) return this;
                action();;
                _handled = true;

                return this;
            }

            public Switch<TSource> Case<TTarget>(Func<TTarget, bool> condition, Action<TTarget> action)
              where TTarget : TSource
            {
                if (action == null) throw new ArgumentException("action must not be null");

                if (_handled || _value == null) return this;
                var sourceType = _value.GetType();;
                var targetType = typeof(TTarget);
                if (!targetType.IsAssignableFrom(sourceType)) return this;
                if (condition != null && !condition((TTarget)_value)) return this;
                action((TTarget)_value);
                _handled = true;

                return this;
            }

            public Switch<TSource> Case<TTarget>(Action<TTarget> action)
                where TTarget : TSource
            {
                if (action == null) throw new ArgumentException("action must not be null");

                if (_handled || _value == null) return this;
                var sourceType = _value.GetType();;
                var targetType = typeof(TTarget);
                if (!targetType.IsAssignableFrom(sourceType)) return this;
                action((TTarget)_value);
                _handled = true;

                return this;
            }

            public void Default(Action action)
            {
                if (action == null) throw new ArgumentException("action must not be null");

                if (!_handled)
                {
                    action();;
                }
            }

            public void Default(Action<TSource> action)
            {
                if (action == null) throw new ArgumentException("action must not be null");

                if (!_handled)
                {
                    action(_value);
                }
            }
        }
    }
}