var Compilify;
(function (Compilify) {
    'use strict';
    var global = (new Function('return this')());
    var _ = global._;
    var delimiter = /\s+/;

    var EventBus = (function () {
        function EventBus() { }
        EventBus.prototype.on = function (eventKeys, callback, context) {
            /// <summary>
            /// Bind one or more events to a callback function. Passing "all" will bind the callback to all
            /// events fired.
            /// </summary>
                        var calls;
            var event;
            var list;

            if(callback) {
                calls = this._callbacks || (this._callbacks = {
                });
                var keys = eventKeys.split(delimiter);
                while(event = keys.shift()) {
                    list = calls[event] || (calls[event] = []);
                    list.push(callback, context);
                }
            }
        };
        EventBus.prototype.off = function (eventKeys, callback, context) {
            /// <summary>
            /// Remove one or many callbacks. If `context` is null, removes all callbacks with that
            /// function. If `callback` is null, removes all callbacks for the event. If `events` is null,
            /// removes all bound callbacks for all events.
            /// </summary>
                        var event;
            var calls;
            var list;
            var i;

            if(!(calls = this._callbacks)) {
                return;
            }
            if(!(eventKeys || callback || context)) {
                delete this._callbacks;
                return;
            }
            var events = eventKeys ? eventKeys.split(delimiter) : _.keys(calls);
            while(event = events.shift()) {
                if(!(list = calls[event]) || !(callback || context)) {
                    delete calls[event];
                    continue;
                }
                for(i = list.length - 2; i >= 0; i -= 2) {
                    if(!(callback && list[i] !== callback || context && list[i + 1] !== context)) {
                        list.splice(i, 2);
                    }
                }
            }
        };
        EventBus.prototype.trigger = function (eventKeys) {
            var rest = [];
            for (var _i = 0; _i < (arguments.length - 1); _i++) {
                rest[_i] = arguments[_i + 1];
            }
            /// <summary>
            /// Trigger one or many events, firing all bound callbacks. Callbacks are passed the same
            /// arguments as `trigger` is, apart from the event name (unless you're listening on `"all"`,
            /// which will cause your callback to receive the true name of the event as the first argument).
            /// </summary>
                        var event;
            var calls;
            var list;
            var i;
            var length;
            var args;
            var all;

            if(!(calls = this._callbacks)) {
                return;
            }
            eventKeys = eventKeys.split(delimiter);
            while(event = eventKeys.shift()) {
                if(all = calls.all) {
                    all = all.slice();
                }
                if(list = calls[event]) {
                    list = list.slice();
                }
                if(list) {
                    for(i = 0 , length = list.length; i < length; i += 2) {
                        list[i].apply(list[i + 1] || this, rest);
                    }
                }
                if(all) {
                    args = [
                        event
                    ].concat(rest);
                    for(i = 0 , length = all.length; i < length; i += 2) {
                        all[i].apply(all[i + 1] || this, args);
                    }
                }
            }
        };
        return EventBus;
    })();
    Compilify.EventBus = EventBus;    
    Compilify.Events = new EventBus();
    Compilify.Events.on('all', function (eventKey) {
        var args = [];
        for (var _i = 0; _i < (arguments.length - 1); _i++) {
            args[_i] = arguments[_i + 1];
        }
        // For debugging event messages, etc...
        console.log('Event: "' + eventKey + '"\t\t', args);
    });
})(Compilify || (Compilify = {}));

