module Compilify {
    'use strict';

    var global = (new Function('return this')()),
        _ = global._,
        delimiter = /\s+/;

    export class EventBus {
        private _callbacks: any;

        on(eventKeys: string, callback: (...args: any[]) => void, context?: any): void {
            /// <summary>
            /// Bind one or more events to a callback function. Passing "all" will bind the callback to all 
            /// events fired.
            /// </summary>

            var calls, event, list;

            if (callback) {
                calls = this._callbacks || (this._callbacks = {});

                var keys = eventKeys.split(delimiter);
                while (event = keys.shift()) {
                    list = calls[event] || (calls[event] = []);
                    list.push(callback, context);
                }
            }
        }

        off(eventKeys: string, callback: (...args: any[]) => void, context?: any): void {
            /// <summary>
            /// Remove one or many callbacks. If `context` is null, removes all callbacks with that 
            /// function. If `callback` is null, removes all callbacks for the event. If `events` is null, 
            /// removes all bound callbacks for all events.
            /// </summary>

            var event, calls, list, i;

            if (!(calls = this._callbacks)) {
                return;
            }

            if (!(eventKeys || callback || context)) {
                delete this._callbacks;
                return;
            }

            var events = eventKeys ? eventKeys.split(delimiter) : _.keys(calls);

            while (event = events.shift()) {
                if (!(list = calls[event]) || !(callback || context)) {
                    delete calls[event];
                    continue;
                }

                for (i = list.length - 2; i >= 0; i -= 2) {
                    if (!(callback && list[i] !== callback || context && list[i + 1] !== context)) {
                        list.splice(i, 2);
                    }
                }
            }
        }

        public trigger(eventKeys, ...rest: any[]): void {
            /// <summary>
            /// Trigger one or many events, firing all bound callbacks. Callbacks are passed the same 
            /// arguments as `trigger` is, apart from the event name (unless you're listening on `"all"`, 
            /// which will cause your callback to receive the true name of the event as the first argument).
            /// </summary>

            var event, calls, list, i, length, args, all;
            if (!(calls = this._callbacks)) {
                return;
            }

            eventKeys = eventKeys.split(delimiter);

            while (event = eventKeys.shift()) {
                if (all = calls.all) {
                    all = all.slice();
                }

                if (list = calls[event]) {
                    list = list.slice();
                }

                if (list) {
                    for (i = 0, length = list.length; i < length; i += 2) {
                        list[i].apply(list[i + 1] || this, rest);
                    }
                }

                if (all) {
                    args = [event].concat(rest);
                    for (i = 0, length = all.length; i < length; i += 2) {
                        all[i].apply(all[i + 1] || this, args);
                    }
                }
            }
        }
    }

    export var Events = new EventBus();
    
    Events.on('all', function(eventKey: string, ...args: any[]) {
        // For debugging event messages, etc...
        console.log('Event: "' + eventKey + '"\t\t', args);
    });
}
