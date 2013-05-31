//     Transition.cs
//     (c) 2013 Brett Ernst, Jameson Ernst, Robert Marsters, Gabriel Isenberg https://github.com/gisenberg/tabletop.io.gui
//     Licensed under the terms of the MIT license.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tabletop.io.Gui {
    public abstract class Transition : IUpdate, IDisposable {
        // static members to support keeping track of groups of tagged transitions
        protected static Dictionary<object, List<Transition>> _running = new Dictionary<object, List<Transition>>();

        public static bool Any (object tag) {
            return _running.ContainsKey(tag) && _running[tag].Count > 0;
        }

        public static Transition<T> New<T> (float duration, T from, T to, Func<T, T, float, T> func, Action<T> callback) where T : struct {
            return new Transition<T>(duration, from, to, func, callback);
        }

        public static Transition<float> New (float duration, Action<float> callback) {
            return new Transition<float>(duration, 0f, 1f, Tween.Lerp, callback);
        }

        public static Transition<float> New (float duration) {
            return Transition.New(duration, null);
        }

        public static void KillAll (object tag) {
            List<Transition> stopList;
            if(_running.TryGetValue(tag, out stopList)) {
                int count;
                while((count = stopList.Count) > 0)
                    stopList[count - 1].Dispose();
            }
        }

        Action _done;
        object _tag;
        float _duration, _current;
        bool _isPaused, _isReverse;
        RepeatType _repeat;
        DepNode _subject;

        public bool IsFinished { get; protected set; }

        internal Transition (float duration) {
            _duration = duration;
            _repeat = RepeatType.Once;
            _isPaused = true;
        }

        public Transition Then (Action callback) {
            if (_done != null) {
                var done = _done;
                _done = () => {
                    done();
                    callback();
                };
            } else
                _done = callback;
            return this;
        }

        public Transition<U> Then<U> (float duration, U from, U to, Func<U, U, float, U> func, Action<U> callback) where U : struct {
            var trn = new Transition<U>(duration, from, to, func, callback);
            var done = _done;
            _done = () => {
                if (done != null)
                    done();
                trn.Reset();
                Gui.Instance.AddUpdateHook(trn);
            };
            return trn;
        }

        public Transition<float> Then (float duration, Action<float> callback) {
            var trn = new Transition<float>(duration, 0f, 1f, Tween.Lerp, callback);
            var done = _done;
            _done = () => {
                if (done != null)
                    done();
                trn.Reset();
                Gui.Instance.AddUpdateHook(trn);
            };
            return trn;
        }

        public Transition<float> Then (float duration) {
            return this.Then(duration, null);
        }

        public Transition Tag (object tag) {
            if (_tag != null && _running.ContainsKey(tag))
                _running[_tag].Remove(this);

            _tag = tag;
            if (!_running.ContainsKey(tag))
                _running.Add(tag, new List<Transition>());
            _running[tag].Add(this);
            return this;
        }

        public Transition Uses (UnityEngine.Object subject) {
            if (_subject == null)
                _subject = new DepNode { subject = subject };
            else {
                var s = _subject;
                while (s.next != null)
                    s = s.next;
                s.next = new DepNode { subject = subject };
            }
            return this;
        }

        public Transition Uses (IControl subject) {
            return this.Uses(subject.Visual);
        }

        public Transition WaitFor (Transition trn) {
            if (!trn.IsFinished) {
                trn.Then(() => Gui.Instance.AddUpdateHook(this));
            }
            return this;
        }

        public Transition Repeat () {
            _repeat = RepeatType.Repeat;
            Gui.Instance.AddUpdateHook(this);
            this.Reset();
            return this;
        }

        public Transition PingPong () {
            _repeat = RepeatType.PingPong;
            Gui.Instance.AddUpdateHook(this);
            this.Reset();
            return this;
        }

        public Transition PingPong (Action done) {
            _done = done;
            Gui.Instance.AddUpdateHook(this);
            return this.PingPong();
        }

        public Transition Now () {
            Gui.Instance.AddUpdateHook(this);
            this.Reset();
            return this;
        }

        public void Pause () {
            _isPaused = true;
        }

        public void Resume () {
            _isPaused = false;
        }

        public void Dispose () {
            if (_tag != null)
                _running[_tag].Remove(this);
            this.IsFinished = true;
            Gui.Instance.RemoveUpdateHook(this);
        }

        public void Dispose (bool skipToEnd) {
            if (skipToEnd) {
                var value = Mathf.Clamp(_current / _duration, 0f, 1f);
                if (value < 1f)
                    this.DoCallback(1f);
            }
            this.Dispose();
        }

        protected abstract void OnCallback (float val);

        protected void Reset () {
            _current = 0f;
            this.DoCallback(0f);
            _isPaused = false;
        }

        void DoCallback (float val) {
            var s = _subject;
            while (s != null) {
                if (s.subject == null) {
                    this.Dispose();
                    return;
                }
                s = s.next;
            }
            this.OnCallback(val);
        }

        void IUpdate.Update (float deltaTime) {
            if (_isPaused)
                return;

            // if any dependencies have been destroyed, stop the transition
            var s = _subject;
            while (s != null) {
                if (s.subject == null) {
                    this.Dispose();
                    return;
                }
                s = s.next;
            }

            if (_isReverse)
                _current -= deltaTime;
            else
                _current += deltaTime;

            var isDone = false;
            var value = Mathf.Clamp(_current / _duration, 0f, 1f);
            if (value <= 0f && _repeat == RepeatType.PingPong) {
                _current = 0f;
                _isReverse = false;
                isDone = true;
            } else if (value >= 1f) {
                switch (_repeat) {
                case RepeatType.Once:
                    isDone = true;
                    break;
                case RepeatType.Repeat:
                    _current = 0f;
                    break;
                case RepeatType.PingPong:
                    _current = _duration;
                    _isReverse = true;
                    break;
                }
            }
            this.DoCallback(value);
            if (isDone) {
                if (_done != null)
                    _done();
                if (_repeat == RepeatType.Once)
                    this.Dispose();
            }
        }

        enum RepeatType {
            Once,
            Repeat,
            PingPong
        }

        class DepNode {
            public UnityEngine.Object subject;
            public DepNode next;
        }
    }

    public class Transition<T> : Transition where T : struct {
        T _from, _to;
        Func<T, T, float, T> _func;
        Action<T> _callback;

        internal Transition (float duration, T from, T to, Func<T, T, float, T> func, Action<T> callback)
            : base(duration) {
            _from = from;
            _to = to;
            _func = func;
            _callback = callback;
        }

        protected override void OnCallback (float val) {
            if(_callback != null)
                _callback(_func(_from, _to, val));
        }
    }
}
