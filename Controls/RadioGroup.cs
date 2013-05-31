//     RadioGroup.cs
//     (c) 2013 Brett Ernst, Jameson Ernst, Robert Marsters, Gabriel Isenberg https://github.com/gisenberg/tabletop.io.gui
//     Licensed under the terms of the MIT license.

using System;
using System.Collections.Generic;
using Tabletop.io.Linq;

namespace Tabletop.io.Gui {
    public class RadioGroup {
        List<Button> _buttons;
        bool _suppress;

        public RadioGroup () {
            _buttons = new List<Button>();
        }

        public Button Active {
            get {
                return _buttons.FirstOrDefault(b => b.IsActive);
            }
        }

        public EventHandler<RadioGroupEventArgs> Changed;

        internal void Add (Button button) {
            _buttons.Add(button);
            button.StateChanged += OnStateChanged;
        }

        internal bool Remove (Button button) {
            button.StateChanged -= OnStateChanged;
            return _buttons.Remove(button);
        }

        internal void Clear () {
            _suppress = true;
            foreach (var b in _buttons)
                b.IsActive = false;
            _suppress = false;
        }

        void OnStateChanged (object sender, ButtonStateChangedEventArgs e) {
            var button = (Button)sender;
            if (e.State == ButtonState.Active) {
                Button previous = null;
                foreach (var other in _buttons)
                    if (other != button && other.IsActive) {
                        other.IsActive = false;
                        previous = other;
                    }
                if (this.Changed != null)
                    this.Changed(this, new RadioGroupEventArgs(button, previous));
            } else if (!_suppress && (e.OldState == ButtonState.Active || e.OldState == ButtonState.ActiveClickOut || e.OldState == ButtonState.ActivePressed)) {
                foreach (var other in _buttons)
                    if (other.IsActive)
                        return;
                button.IsActive = true;
            }
        }
    }

    public class RadioGroupEventArgs : EventArgs {
        public Button Previous { get; private set; }
        public Button Active { get; private set; }

        internal RadioGroupEventArgs (Button active, Button previous) {
            this.Active = active;
            this.Previous = previous;
        }
    }
}
