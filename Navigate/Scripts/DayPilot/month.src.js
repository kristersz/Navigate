/*
Copyright © 2013 Annpoint, s.r.o.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

-------------------------------------------------------------------------

NOTE: Reuse requires the following acknowledgement (see also NOTICE):
This product includes DayPilot (http://www.daypilot.org) developed by Annpoint, s.r.o.
*/


if (typeof DayPilot === 'undefined') {
	var DayPilot = {};
}

// compatibility with 5.9.2029 and previous
if (typeof DayPilotMonth === 'undefined') {
	var DayPilotMonth = DayPilot.MonthVisible = {};
}

(function() {

    var doNothing = function() { };

    if (typeof DayPilot.Month !== 'undefined') {
        return;
    }

    var DayPilotMonth = {};

    DayPilotMonth.Month = function(placeholder) {
        this.nav = {};
        this.nav.top = document.getElementById(placeholder);

        var calendar = this;

        this.id = placeholder;
        this.isMonth = true;

        this.hideUntilInit = true;

        this.startDate = new DayPilot.Date(); // today
        this.width = '100%'; // default width is 100%
        this.cellHeight = 100; // default cell height is 100 pixels (it's a minCellHeight, it will be extended if needed)
        this.eventFontColor = "#000000";
        this.eventFontFamily = "Tahoma";
        this.eventFontSize = "11px";
        //this.events = [];
        this.headerBackColor = '#F3F3F9';
        this.headerFontColor = '#42658C';
        this.headerFontFamily = "Tahoma";
        this.headerFontSize = "10pt";
        this.headerHeight = 20;
        this.monthNames = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"]; // default month names
        this.dayNames = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
        this.weekStarts = 1; // Monday
        this.innerBorderColor = '#cccccc';
        this.borderColor = '#CED2CE';
        this.eventHeight = 20;
        this.cellHeaderHeight = 16;

        this.afterRender = function() { };
        this.backColor = '#ffffff';
        this.nonBusinessBackColor = '#ffffff';
        this.cellHeaderBackColor = '#ffffff';
        this.cellHeaderFontColor = '#42658C';
        this.cellHeaderFontFamily = 'Tahoma';
        this.cellHeaderFontSize = '10pt';
        this.eventBackColor = '#2951A5';
        this.eventBorderColor = '#2951A5';
        this.eventFontColor = '#ffffff';
        this.eventFontFamily = 'Tahoma';
        this.eventFontSize = '11px';
        this.eventHeight = 16;
        this.cellWidth = 14.285; // internal, 7 cells per row
        this.lineSpace = 1;

        this.eventTimeFontColor = 'gray';
        this.eventTimeFontFamily = 'Tahoma';
        this.eventTimeFontSize = '8pt';

        this.eventClickHandling = 'Disabled';
        this.eventMoveHandling = 'Disabled';
        this.eventResizeHandling = 'Disabled';
        this.timeRangeSelectedHandling = 'Disabled';

        this.backendUrl = null;
        this.cellEvents = [];

        this.elements = {};
        this.elements.events = [];

        this.cache = {};
        this.cache.events = {}; // register DayPilotMonth.Event objects here, key is the data event, reset during drawevents

        this.updateView = function(result, context) {

            var result = eval("(" + result + ")");

            if (result.UpdateType === "None") {
                calendar.fireAfterRenderDetached(result.CallBackData, true);
                return;
            }

            calendar.events = result.Events;

            if (result.UpdateType === "Full") {

                // properties
                calendar.startDate = result.StartDate;
                calendar.headerBackColor = result.HeaderBackColor ? result.HeaderBackColor : calendar.headerBackColor;
                calendar.backColor = result.BackColor ? result.BackColor : calendar.backColor;
                calendar.nonBusinessBackColor = result.NonBusinessBackColor ? result.NonBusinessBackColor : calendar.nonBusinessBackColor;
                calendar.timeFormat = result.TimeFormat ? result.TimeFormat : calendar.timeFormat;
                if (typeof result.WeekStarts !== 'undefined') { calendar.weekStarts = result.WeekStarts; } // number, can be 0

                calendar.hashes = result.Hashes;
            }

            calendar.clearEvents();
            calendar.prepareRows();
            calendar.prepareEvents();

            if (result.UpdateType === "Full") {
                calendar.clearTable();
                calendar.drawTable();
            }
            calendar.updateHeight();

            calendar.show();

            calendar.drawEvents();

            calendar.fireAfterRenderDetached(result.CallBackData, true);

        };

        this.fireAfterRenderDetached = function(data, isCallBack) {
            var afterRenderDelayed = function(data, isc) {
                return function() {
                    if (calendar.afterRender) {
                        calendar.afterRender(data, isc);
                    }
                };
            };

            window.setTimeout(afterRenderDelayed(data, isCallBack), 0);
        };

        this.lineHeight = function() {
            return this.eventHeight + this.lineSpace;
        };

        this.prepareEvents = function() {
            // prepare rows and columns
            for (var x = 0; x < this.events.length; x++) {
                var e = this.events[x];
                e.Start = new DayPilot.Date(e.Start).d;
                e.End = new DayPilot.Date(e.End).d;
                if (e.Start.getTime() > e.End.getTime()) { // skip invalid events, zero duration allowed
                    continue;
                }
                for (var i = 0; i < this.rows.length; i++) {
                    var row = this.rows[i];
                    if (row.belongsHere(e)) {
                        row.events.push(e);
                    }
                }
            }

            // arrange events into lines
            for (var ri = 0; ri < this.rows.length; ri++) {
                var row = this.rows[ri];
                row.events.sort(this.eventComparer);

                for (var ei = 0; ei < this.rows[ri].events.length; ei++) {
                    var ev = row.events[ei];
                    var colStart = row.getStartColumn(ev);
                    var colWidth = row.getWidth(ev);
                    var line = row.putIntoLine(ev, colStart, colWidth, ri);
                }
            }

        };


        this.clearEvents = function() {
            for (var i = 0; i < this.elements.events.length; i++) {
                var e = this.elements.events[i];
                e.event = null;
                e.click = null;
                e.parentNode.removeChild(e);
            }

            this.elements.events = [];

        };

        this.drawEvents = function() {
            this.cache.events = {};  // reset DayPilotMonth.Event object cache

            this.drawEventsRows();
        };

        this.drawEventsRows = function() {
            this.elements.events = [];

            // draw events
            for (var ri = 0; ri < this.rows.length; ri++) {
                var row = this.rows[ri];

                for (var li = 0; li < row.lines.length; li++) {
                    var line = row.lines[li];

                    for (var pi = 0; pi < line.length; pi++) {
                        this.drawEvent(line[pi]);
                    }
                }
            }

        };

        this.eventComparer = function(a, b) {
            if (!a || !b || !a.Start || !b.Start) {
                return 0; // no sorting, invalid arguments
            }

            var byStart = a.Start.getTime() - b.Start.getTime();
            if (byStart !== 0) {
                return byStart;
            }

            var byEnd = b.End.getTime() - a.End.getTime(); // desc
            return byEnd;
        };

        this.drawShadow = function(x, y, line, width, offset, e) {

            if (!offset) {
                offset = 0;
            }

            var remains = width;

            this.shadow = {};
            this.shadow.list = [];
            this.shadow.start = { x: x, y: y };
            this.shadow.width = width;

            // something before the first day
            var hidden = y * 7 + x - offset;
            if (hidden < 0) {
                //document.title = hidden + ' ' + new Date();
                remains += hidden;
                x = 0;
                y = 0;
            }

            var remainingOffset = offset;
            while (remainingOffset >= 7) {
                y--;
                remainingOffset -= 7;
            }
            if (remainingOffset > x) {
                var plus = 7 - this.getColCount();
                if (remainingOffset > (x + plus)) {
                    y--;
                    x = x + 7 - remainingOffset;
                }
                else {
                    remains = remains - remainingOffset + x;
                    x = 0;
                }
            }
            else {
                x -= remainingOffset;
            }

            if (y < 0) {
                y = 0;
                x = 0;
            }

            var cursor = null;
            if (DayPilotMonth.resizingEvent) {
                cursor = 'w-resize';
            }
            else if (DayPilotMonth.movingEvent) {
                cursor = "move";
            }

            this.nav.top.style.cursor = cursor;

            while (remains > 0 && y < this.rows.length) {
                var drawNow = Math.min(this.getColCount() - x, remains);
                var row = this.rows[y];


                var top = this.getRowTop(y);
                var height = row.getHeight();

                var shadow = document.createElement("div");
                shadow.setAttribute("unselectable", "on");
                shadow.style.position = 'absolute';
                shadow.style.left = (this.getCellWidth() * x) + '%';
                shadow.style.width = (this.getCellWidth() * drawNow) + '%';
                shadow.style.top = (top) + 'px';
                shadow.style.height = (height) + 'px';
                shadow.style.cursor = cursor;

                var inside = document.createElement("div");
                inside.setAttribute("unselectable", "on");
                shadow.appendChild(inside);

                inside.style.position = "absolute";
                inside.style.top = "0px";
                inside.style.right = "0px";
                inside.style.left = "0px";
                inside.style.bottom = "0px";

                inside.style.backgroundColor = "#aaaaaa";
                inside.style.opacity = 0.5;
                inside.style.filter = "alpha(opacity=50)";
                //inside.style.border = '2px solid #aaaaaa';
                if (e && e.eventPart && e.eventPart.event) {
                    inside.style.overflow = 'hidden';
                    inside.style.fontSize = this.eventFontSize;
                    inside.style.fontFamily = this.eventFontFamily;
                    inside.style.color = this.eventFontColor;
                    inside.innerHTML = e.eventPart.event.InnerHTML ? e.eventPart.event.InnerHTML : e.eventPart.event.Text;
                }


                this.nav.top.appendChild(shadow);
                this.shadow.list.push(shadow);

                remains -= (drawNow + 7 - this.getColCount());
                x = 0;
                y++;
            }

        };

        this.clearShadow = function() {
            if (this.shadow) {
                for (var i = 0; i < this.shadow.list.length; i++) {
                    this.nav.top.removeChild(this.shadow.list[i]);
                }
                this.shadow = null;
                this.nav.top.style.cursor = '';
            }
        };

        this.getEventTop = function(row, line) {
            var top = this.headerHeight;
            for (var i = 0; i < row; i++) {
                top += this.rows[i].getHeight();
            }
            top += this.cellHeaderHeight; // space on top
            top += line * this.lineHeight();
            return top;
        };

        this.getDateFromCell = function(x, y) {
            return DayPilot.Date.addDays(this.firstDate, y * 7 + x);
        };

        this.drawEvent = function(eventPart) {

            var ev = eventPart.event;
            var row = eventPart.row;
            var line = eventPart.line;
            var colStart = eventPart.colStart;
            var colWidth = eventPart.colWidth;

            var left = this.getCellWidth() * (colStart);
            var width = this.getCellWidth() * (colWidth);
            var top = this.getEventTop(row, line);

            var e = document.createElement("div");
            e.setAttribute("unselectable", "on");
            e.style.height = this.eventHeight + 'px';

            e.style.fontFamily = this.eventFontFamily;

            var eo = null;
            var textId = eventPart.event.Value;
            if (this.cache.events[textId]) {
                eo = this.cache.events[textId];
            }
            else {
                eo = new DayPilotMonth.Event(calendar, eventPart, row);
                this.cache.events[textId] = eo;
            }
            e.event = eo;

            e.style.width = width + '%';
            e.style.position = 'absolute';
            e.style.left = left + '%';
            e.style.top = top + 'px'; // plus space on top

            if (this.showToolTip && ev.ToolTip) {
                e.title = ev.ToolTip;
            }

            e.onclick = calendar.eventClickDispatch;
            e.onmousedown = function(ev) {
                ev = ev || window.event;
                var button = ev.which || ev.button;

                ev.cancelBubble = true;
                if (ev.stopPropagation) {
                    ev.stopPropagation();
                }

                if (button === 1) {

                    DayPilotMonth.movingEvent = null;
                    if (this.style.cursor === 'w-resize' || this.style.cursor === 'e-resize') {
                        var resizing = {};
                        resizing.start = {};
                        resizing.start.x = colStart;
                        resizing.start.y = row;
                        resizing.event = e.event;
                        resizing.width = DayPilot.Date.daysSpan(resizing.event.start().d, resizing.event.end().d) + 1;
                        resizing.direction = this.style.cursor;
                        DayPilotMonth.resizingEvent = resizing;
                    }
                    else if (this.style.cursor === 'move' || calendar.eventMoveHandling !== 'Disabled') {
                        calendar.clearShadow();

                        var coords = DayPilot.mo2(calendar.nav.top, ev);
                        if (!coords) {
                            return;
                        }

                        var cell = calendar.getCellBelowPoint(coords.x, coords.y);

                        var hidden = DayPilot.Date.daysDiff(eventPart.event.Start, calendar.rows[row].start);
                        var offset = (cell.y * 7 + cell.x) - (row * 7 + colStart);
                        if (hidden) {
                            offset += hidden;
                        }

                        var moving = {};
                        moving.start = {};
                        moving.start.x = colStart;
                        moving.start.y = row;
                        moving.start.line = line;
                        moving.offset = calendar.eventMoveToPosition ? 0 : offset;
                        moving.colWidth = colWidth;
                        moving.event = e.event;
                        moving.coords = coords;
                        DayPilotMonth.movingEvent = moving;

                    }

                }
            };

            e.onmousemove = function(ev) {
                if (typeof (DayPilotMonth) === 'undefined') {
                    return;
                }

                if (DayPilotMonth.movingEvent || DayPilotMonth.resizingEvent) {
                    return;
                }

                // position
                var offset = DayPilot.mo3(e, ev);
                if (!offset) {
                    return;
                }
                
                var resizeMargin = 6;

                if (offset.x <= resizeMargin && calendar.eventResizeHandling !== 'Disabled') {
                    if (eventPart.startsHere) {
                        e.style.cursor = "w-resize";
                        e.dpBorder = 'left';
                    }
                    else {
                        e.style.cursor = 'not-allowed';
                    }
                }
                else if (e.clientWidth - offset.x <= resizeMargin && calendar.eventResizeHandling !== 'Disabled') {
                    if (eventPart.endsHere) {
                        e.style.cursor = "e-resize";
                        e.dpBorder = 'right';
                    }
                    else {
                        e.style.cursor = 'not-allowed';
                    }
                }
                else {
                    e.style.cursor = 'default';
                }

            };

            e.onmouseout = function(ev) {
                e.style.cursor = '';
            };

            var back = (ev.BackColor) ? ev.BackColor : this.eventBackColor;

            var inner = document.createElement("div");
            inner.setAttribute("unselectable", "on");
            inner.style.height = (this.eventHeight - 2) + 'px';
            inner.style.overflow = 'hidden';
            inner.style.position = "absolute";
            inner.style.left = "2px";
            inner.style.right = "2px";

            inner.style.paddingLeft = '2px';
            inner.style.border = '1px solid ' + calendar.eventBorderColor;
            inner.style.backgroundColor = back;
            inner.style.fontFamily = "";
            //inner.className = this.prefixCssClass("event");

            inner.style.MozBorderRadius = "5px";
            inner.style.webkitBorderRadius = "5px";
            inner.style.borderRadius = "5px";

            var inside = [];

            inside.push("<div unselectable='on' style='");
            inside.push("font-size:");
            inside.push(this.eventFontSize);
            inside.push(";color:");
            inside.push(this.eventFontColor);
            inside.push(";font-family:");
            inside.push(this.eventFontFamily);
            //inside.push(";text-align:center");
            inside.push(";'>");
            inside.push(ev.Text);
            inside.push("</div>");

            inner.innerHTML = inside.join('');

            e.appendChild(inner);

            this.elements.events.push(e);

            this.nav.events.appendChild(e);
        };


        // returns DayPilot.Date object
        this.lastVisibleDayOfMonth = function() {
            return  this.startDate.lastDayOfMonth();
        };

        this.prepareRows = function() {

            if (typeof this.startDate === 'string') {
                this.startDate = DayPilot.Date.fromStringSortable(this.startDate);
            }
            this.startDate = this.startDate.firstDayOfMonth();

            this.firstDate = this.startDate.firstDayOfWeek(this.getWeekStart());

            var firstDayOfMonth = this.startDate;

            var rowCount;

            var lastVisibleDayOfMonth = this.lastVisibleDayOfMonth().d;
            var count = DayPilot.Date.daysDiff(this.firstDate, lastVisibleDayOfMonth) + 1;
            rowCount = Math.ceil(count / 7);

            this.days = rowCount * 7;

            this.rows = [];
            for (var x = 0; x < rowCount; x++) {
                var r = {};
                r.start = DayPilot.Date.addDays(this.firstDate, x * 7);  // start point
                r.end = DayPilot.Date.addDays(r.start, this.getColCount()); // end point
                r.events = []; // collection of events
                r.lines = []; // collection of lines
                r.index = x; // row index
                r.minHeight = this.cellHeight; // default, can be extended during events loading
                r.calendar = this;

                r.belongsHere = function(ev) {
                    if (ev.End.getTime() === ev.Start.getTime() && ev.Start.getTime() === this.start.getTime()) {
                        return true;
                    }
                    return !(ev.End.getTime() <= this.start.getTime() || ev.Start.getTime() >= this.end.getTime());
                };

                r.getPartStart = function(ev) {
                    return DayPilot.Date.max(this.start, ev.Start);
                };

                r.getPartEnd = function(ev) {
                    return DayPilot.Date.min(this.end, ev.End);
                };

                r.getStartColumn = function(ev) {
                    var partStart = this.getPartStart(ev);
                    return DayPilot.Date.daysDiff(this.start, partStart);
                };

                r.getWidth = function(ev) {
                    return DayPilot.Date.daysSpan(this.getPartStart(ev), this.getPartEnd(ev)) + 1;
                };

                r.putIntoLine = function(ev, colStart, colWidth, row) {
                    var thisRow = this;

                    for (var i = 0; i < this.lines.length; i++) {
                        var line = this.lines[i];
                        if (line.isFree(colStart, colWidth)) {
                            line.addEvent(ev, colStart, colWidth, row, i);
                            return i;
                        }
                    }

                    var line = [];
                    line.isFree = function(colStart, colWidth) {
                        var free = true;

                        for (var i = 0; i < this.length; i++) {
                            if (!(colStart + colWidth - 1 < this[i].colStart || colStart > this[i].colStart + this[i].colWidth - 1)) {
                                free = false;
                            }
                        }

                        return free;
                    };

                    line.addEvent = function(ev, colStart, colWidth, row, index) {
                        var eventPart = {};
                        eventPart.event = ev;
                        eventPart.colStart = colStart;
                        eventPart.colWidth = colWidth;
                        eventPart.row = row;
                        eventPart.line = index;
                        eventPart.startsHere = thisRow.start.getTime() <= ev.Start.getTime();
                        //if (confirm('r.start: ' + thisRow.start + ' ev.Start: ' + ev.Start)) thisRow = null;
                        eventPart.endsHere = thisRow.end.getTime() >= ev.End.getTime();

                        this.push(eventPart);
                    };

                    line.addEvent(ev, colStart, colWidth, row, this.lines.length);

                    this.lines.push(line);

                    return this.lines.length - 1;
                };

                r.getStart = function() {
                    var start = 0;
                    for (var i = 0; i < calendar.rows.length && i < this.index; i++) {
                        start += calendar.rows[i].getHeight();
                    }
                };

                r.getHeight = function() {
                    return Math.max(this.lines.length * calendar.lineHeight() + calendar.cellHeaderHeight, this.calendar.cellHeight);
                };

                this.rows.push(r);
            }

            this.endDate = DayPilot.Date.addDays(this.firstDate, rowCount * 7);
        };

        this.getHeight = function() {
            var height = this.headerHeight;
            for (var i = 0; i < this.rows.length; i++) {
                height += this.rows[i].getHeight();
            }
            return height;
        };

        this.getWidth = function(start, end) {
            var diff = (end.y * 7 + end.x) - (start.y * 7 + start.x);
            return diff + 1;
        };

        this.getMinCoords = function(first, second) {
            if ((first.y * 7 + first.x) < (second.y * 7 + second.x)) {
                return first;
            }
            else {
                return second;
            }
        };

        this.drawTop = function() {
            var relative = this.nav.top;
            //this.nav.top = relative;
            relative.setAttribute("unselectable", "on");
            relative.style.MozUserSelect = 'none';
            relative.style.KhtmlUserSelect = 'none';
            relative.style.WebkitUserSelect = 'none';
            relative.style.position = 'relative';
            if (this.width) {
                relative.style.width = this.width;
            }
            relative.style.height = this.getHeight() + 'px';
            relative.onselectstart = function(e) { return false; }; // prevent text cursor in Chrome during drag&drop

            relative.style.border = "1px solid " + this.borderColor;

            if (this.hideUntilInit) {
                relative.style.visibility = 'hidden';
            }

            var cells = document.createElement("div");
            this.nav.cells = cells;
            cells.style.position = "absolute";
            cells.style.left = "0px";
            cells.style.right = "0px";
            cells.setAttribute("unselectable", "on");
            relative.appendChild(cells);

            var events = document.createElement("div");
            this.nav.events = events;
            events.style.position = "absolute";
            events.style.left = "0px";
            events.style.right = "0px";
            events.setAttribute("unselectable", "on");
            relative.appendChild(events);

            relative.onmousemove = function(ev) {

                if (DayPilotMonth.resizingEvent) {
                    var coords = DayPilot.mo2(calendar.nav.top, ev);

                    if (!coords) {
                        return;
                    }

                    var cell = calendar.getCellBelowPoint(coords.x, coords.y);
                    calendar.clearShadow();
                    var resizing = DayPilotMonth.resizingEvent;

                    var original = resizing.start;
                    var width, start;

                    if (resizing.direction === 'w-resize') {
                        start = cell;

                        var endDate = resizing.event.end().d;
                        if (DayPilot.Date.getDate(endDate).getTime() === endDate.getTime()) {
                            endDate = DayPilot.Date.addDays(endDate, -1);
                        }

                        var end = calendar.getCellFromDate(endDate);
                        width = calendar.getWidth(cell, end);
                    }
                    else {
                        start = calendar.getCellFromDate(resizing.event.start().d);
                        width = calendar.getWidth(start, cell);
                    }

                    if (width < 1) {
                        width = 1;
                    }

                    calendar.drawShadow(start.x, start.y, 0, width);

                }
                else if (DayPilotMonth.movingEvent) {
                    var coords = DayPilot.mo2(calendar.nav.top, ev);

                    if (!coords) {
                        return;
                    }

                    // not actually moved, Chrome bug
                    if (coords.x == DayPilotMonth.movingEvent.coords.x && coords.y == DayPilotMonth.movingEvent.coords.y) {
                        return;
                    }

                    var cell = calendar.getCellBelowPoint(coords.x, coords.y);

                    calendar.clearShadow();

                    var event = DayPilotMonth.movingEvent.event;
                    var offset = DayPilotMonth.movingEvent.offset;
                    var width = calendar.cellMode ? 1 : DayPilot.Date.daysSpan(event.start().d, event.end().d) + 1;

                    if (width < 1) {
                        width = 1;
                    }
                    calendar.drawShadow(cell.x, cell.y, 0, width, offset, event);
                }
                else if (DayPilotMonth.timeRangeSelecting) {
                    var coords = DayPilot.mo2(calendar.nav.top, ev);

                    if (!coords) {
                        return;
                    }

                    var cell = calendar.getCellBelowPoint(coords.x, coords.y);

                    calendar.clearShadow();

                    var start = DayPilotMonth.timeRangeSelecting;

                    var startIndex = start.y * 7 + start.x;
                    var cellIndex = cell.y * 7 + cell.x;

                    var width = Math.abs(cellIndex - startIndex) + 1;

                    if (width < 1) {
                        width = 1;
                    }

                    var shadowStart = startIndex < cellIndex ? start : cell;

                    DayPilotMonth.timeRangeSelecting.from = { x: shadowStart.x, y: shadowStart.y };
                    DayPilotMonth.timeRangeSelecting.width = width;
                    DayPilotMonth.timeRangeSelecting.moved = true;

                    calendar.drawShadow(shadowStart.x, shadowStart.y, 0, width, 0, null);

                }

            };

            //this.nav.top.appendChild(this.vsph);
        };

        this.updateHeight = function() {
            this.nav.top.style.height = this.getHeight() + 'px';

            for (var x = 0; x < this.cells.length; x++) {
                for (var y = 0; y < this.cells[x].length; y++) {
                    this.cells[x][y].style.top = this.getRowTop(y) + 'px';
                    this.cells[x][y].style.height = this.rows[y].getHeight() + 'px';
                }
            }
        };

        this.getCellBelowPoint = function(x, y) {
            var columnWidth = Math.floor(this.nav.top.clientWidth / this.getColCount());
            var column = Math.min(Math.floor(x / columnWidth), this.getColCount() - 1);

            var row = null;

            var height = this.headerHeight;
            var relativeY = 0;
            for (var i = 0; i < this.rows.length; i++) {
                var baseHeight = height;
                height += this.rows[i].getHeight();
                if (y < height) {
                    relativeY = y - baseHeight;
                    row = i;
                    break;
                }
            }
            if (row === null) {
                row = this.rows.length - 1; // might be a pixel below the last line
            }

            var cell = {};
            cell.x = column;
            cell.y = row;
            cell.relativeY = relativeY;

            return cell;
        };

        this.getCellFromDate = function(date) {
            var width = DayPilot.Date.daysDiff(this.firstDate, date);
            var cell = { x: 0, y: 0 };
            while (width >= 7) {
                cell.y++;
                width -= 7;
            }
            cell.x = width;
            return cell;
        };

        this.drawTable = function() {

            var table = document.createElement("div");
            table.oncontextmenu = function() { return false; };
            this.nav.cells.appendChild(table);

            this.cells = [];

            for (var x = 0; x < this.getColCount(); x++) {

                this.cells[x] = [];
                var headerProperties = null;

                var header = document.createElement("div");
                header.setAttribute("unselectable", "on");
                header.style.position = 'absolute';

                header.style.left = (this.getCellWidth() * x) + '%';
                header.style.width = (this.getCellWidth()) + '%';
                header.style.top = '0px';
                header.style.height = (this.headerHeight) + 'px';

                var dayIndex = x + this.getWeekStart();
                if (dayIndex > 6) {
                    dayIndex -= 7;
                }

                var inner = document.createElement("div");
                inner.setAttribute("unselectable", "on");
                inner.innerHTML = this.dayNames[dayIndex];

                header.appendChild(inner);

                inner.style.position = "absolute";
                inner.style.top = "0px";
                inner.style.bottom = "0px";
                inner.style.left = "0px";
                inner.style.right = "0px";
                inner.style.backgroundColor = this.headerBackColor;
                inner.style.fontFamily = this.headerFontFamily;
                inner.style.fontSize = this.headerFontSize;
                inner.style.color = this.headerFontColor;

                inner.style.textAlign = 'center';
                inner.style.cursor = 'default';
                //inner.className = this.prefixCssClass("header");

                if (x !== this.getColCount() - 1) {
                    inner.style.borderRight = '1px solid ' + this.borderColor;
                }
                inner.innerHTML = this.dayNames[dayIndex];

                table.appendChild(header);

                for (var y = 0; y < this.rows.length; y++) {
                    this.drawCell(x, y, table);
                }

            }

        };

        this.clearTable = function() {

            // clear event handlers
            for (var x = 0; x < this.cells.length; x++) {
                for (var y = 0; y < this.cells[x].length; y++) {
                    this.cells[x][y].onclick = null;
                }
            }

            this.nav.cells.innerHTML = '';

        };

        this.drawCell = function(x, y, table) {

            var row = this.rows[y];
            var d = DayPilot.Date.addDays(this.firstDate, y * 7 + x);
            var cellProperties = this.cellProperties ? this.cellProperties[y * this.getColCount() + x] : null;


            var cell = document.createElement("div");
            cell.setAttribute("unselectable", "on");
            cell.style.position = 'absolute';
            cell.style.cursor = 'default';
            cell.style.left = (this.getCellWidth() * x) + '%';
            cell.style.width = (this.getCellWidth()) + '%';
            cell.style.top = (this.getRowTop(y)) + 'px';
            cell.style.height = (row.getHeight()) + 'px';

            var previousMonth = this.startDate.addMonths(-1).getMonth();
            var nextMonth = this.startDate.addMonths(1).getMonth();

            var thisMonth = this.startDate.getMonth();

            var inner = document.createElement("div");
            inner.setAttribute("unselectable", "on");
            cell.appendChild(inner);

            inner.style.position = "absolute";
            inner.style.left = "0px";
            inner.style.right = "0px";
            inner.style.top = "0px";
            inner.style.bottom = "0px";

            /*
            if (d.getUTCMonth() === thisMonth) {
                cell.className = this.prefixCssClass("cell");
            }
            else if (d.getUTCMonth() === previousMonth) {
                cell.className = this.prefixCssClass("cell") + " " + this.prefixCssClass("previous");
            }
            else if (d.getUTCMonth() === nextMonth) {
                cell.className = this.prefixCssClass("cell") + " " + this.prefixCssClass("next");
            }
            else {
                doNothing();
                //cell.className = "d.getMonth():" + d.getMonth() + ":nextMonth:" + nextMonth + ":previousMonth:" + previousMonth;
            }
            */
            inner.style.backgroundColor = this.getCellBackColor(d);

            if (x !== this.getColCount() - 1) {
                inner.style.borderRight = '1px solid ' + this.innerBorderColor;
            }
            else {
                //inner.style.borderLeft = '1px solid ' + this.innerBorderColor;
            }

            if (y === 0) {
                inner.style.borderTop = '1px solid ' + this.borderColor;
            } 

            inner.style.borderBottom = '1px solid ' + this.innerBorderColor;

            if (x === this.getColCount() - 1) {
                //inner.style.borderRight = '1px solid ' + this.borderColor;
            }


            cell.onmousedown = function(e) {
                if (calendar.timeRangeSelectedHandling !== 'Disabled') {
                    calendar.clearShadow();
                    //calendar.drawShadow(x, y, null, 1, 0, null);
                    DayPilotMonth.timeRangeSelecting = { "root": calendar, "x": x, "y": y, "from": { x: x, y: y }, "width": 1 };
                }
            };

            cell.onclick = function() {

                var single = function(d) {
                    var start = new DayPilot.Date(d);
                    var end = start.addDays(1);
                    calendar.timeRangeSelectedDispatch(start, end);
                };

                if (calendar.timeRangeSelectedHandling !== 'Disabled') {
                    single(d);
                    return;
                }

            };

            var day = document.createElement("div");
            day.setAttribute("unselectable", "on");
            day.style.height = this.cellHeaderHeight + "px";

            if (this.cellHeaderBackColor) {
                day.style.backgroundColor = this.cellHeaderBackColor;
            }
            day.style.paddingRight = '2px';
            day.style.textAlign = "right";
            day.style.fontFamily = this.cellHeaderFontFamily;
            day.style.fontSize = this.cellHeaderFontSize;
            day.style.color = this.cellHeaderFontColor;
            //day.className = this.prefixCssClass("cellheader");

            var date = d.getUTCDate();
            if (date === 1) {
                day.innerHTML = this.monthNames[d.getUTCMonth()] + ' ' + date;
            }
            else {
                day.innerHTML = date;
            }

            inner.appendChild(day);

            this.cells[x][y] = cell;

            table.appendChild(cell);
        };

        this.getWeekStart = function() {
            if (this.showWeekend) {
                return this.weekStarts;
            }
            else {
                return 1; // Monday
            }
        };

        this.getColCount = function() {
            return 7;
        };

        this.getCellWidth = function() {
            return 14.285;
        };

        this.getCellBackColor = function(d) {
            if (d.getUTCDay() === 6 || d.getUTCDay() === 0) {
                return this.nonBusinessBackColor;
            }
            return this.backColor;
        };

        this.getRowTop = function(index) {
            var top = this.headerHeight;
            for (var i = 0; i < index; i++) {
                top += this.rows[i].getHeight();
            }
            return top;
        };

        this.callBack2 = function(action, data, parameters) {

            var envelope = {};

            envelope.action = action;
            envelope.parameters = parameters;
            envelope.data = data;
            envelope.header = this.getCallBackHeader();

            var commandstring = "JSON" + DayPilot.JSON.stringify(envelope);

            if (this.backendUrl) {
                DayPilot.request(this.backendUrl, this.callBackResponse, commandstring, this.ajaxError);
            }
        };

        this.callBackResponse = function(response) {
            calendar.updateView(response.responseText);
        };

        this.getCallBackHeader = function() {
            var h = {};
            h.control = "dpm";
            h.id = this.id;
            h.visibleStart = new DayPilot.Date(this.firstDate);
            h.visibleEnd = h.visibleStart.addDays(this.days);

            h.startDate = calendar.startDate;
            h.headerBackColor = this.headerBackColor;
            h.backColor = this.backColor;
            h.nonBusinessBackColor = this.nonBusinessBackColor;
            h.timeFormat = this.timeFormat;
            h.weekStarts = this.weekStarts;

            return h;
        };

        this.eventClickCallBack = function(e, data) {
            this.callBack2('EventClick', data, e);
        };

        this.eventClickDispatch = function(e) {

            DayPilotMonth.movingEvent = null;
            DayPilotMonth.resizingEvent = null;

            var div = this;

            var e = e || window.event;
            var ctrlKey = e.ctrlKey;

            e.cancelBubble = true;
            if (e.stopPropagation) {
                e.stopPropagation();
            }

            calendar.eventClickSingle(div, ctrlKey);
        };


        this.eventClickSingle = function(div, ctrlKey) {
            var e = div.event;
            if (!e.clickingAllowed()) {
                return;
            }

            switch (calendar.eventClickHandling) {
                case 'CallBack':
                    calendar.eventClickCallBack(e);
                    break;
                case 'JavaScript':
                    calendar.onEventClick(e);
                    break;
            }
        };

        this.eventMoveCallBack = function(e, newStart, newEnd, data, position) {
            if (!newStart)
                throw 'newStart is null';
            if (!newEnd)
                throw 'newEnd is null';

            var params = {};
            params.e = e;
            params.newStart = newStart;
            params.newEnd = newEnd;
            params.position = position;

            this.callBack2('EventMove', data, params);
        };

        this.eventMoveDispatch = function(e, x, y, offset, ev, position) {

            var startOffset = DayPilot.Date.getTime(e.start().d);

            var endDate = DayPilot.Date.getDate(e.end().d);
            if (endDate.getTime() !== e.end().d.getTime()) {
                endDate = DayPilot.Date.addDays(endDate, 1);
            }
            var endOffset = DayPilot.Date.diff(e.end().d, endDate);

            var boxStart = this.getDateFromCell(x, y);
            boxStart = DayPilot.Date.addDays(boxStart, -offset);
            var width = DayPilot.Date.daysSpan(e.start().d, e.end().d) + 1;

            var boxEnd = DayPilot.Date.addDays(boxStart, width);

            var newStart = new DayPilot.Date(DayPilot.Date.addTime(boxStart, startOffset));
            var newEnd = new DayPilot.Date(DayPilot.Date.addTime(boxEnd, endOffset));

            switch (calendar.eventMoveHandling) {
                case 'CallBack':
                    calendar.eventMoveCallBack(e, newStart, newEnd, null, position);
                    break;
                case 'JavaScript':
                    calendar.onEventMove(e, newStart, newEnd, ev.ctrlKey, ev.shiftKey, position);
                    break;
            }

        };

        this.eventResizeCallBack = function(e, newStart, newEnd, data) {
            if (!newStart)
                throw 'newStart is null';
            if (!newEnd)
                throw 'newEnd is null';

            var params = {};
            params.e = e;
            params.newStart = newStart;
            params.newEnd = newEnd;

            this.callBack2('EventResize', data, params);
        };

        this.eventResizeDispatch = function(e, start, width) {
            var startOffset = DayPilot.Date.getTime(e.start().d);

            var endDate = DayPilot.Date.getDate(e.end().d);
            if (!DayPilot.Date.equals(endDate, e.end().d)) {
                endDate = DayPilot.Date.addDays(endDate, 1);
            }
            var endOffset = DayPilot.Date.diff(e.end().d, endDate);

            var boxStart = this.getDateFromCell(start.x, start.y);
            //var width = DayPilot.Date.daysSpan(e.start(), e.end()) + 1;
            var boxEnd = DayPilot.Date.addDays(boxStart, width);

            var newStart = new DayPilot.Date(DayPilot.Date.addTime(boxStart, startOffset));
            var newEnd = new DayPilot.Date(DayPilot.Date.addTime(boxEnd, endOffset));

            switch (calendar.eventResizeHandling) {
                case 'CallBack':
                    calendar.eventResizeCallBack(e, newStart, newEnd);
                    break;
                case 'JavaScript':
                    calendar.onEventResize(e, newStart, newEnd);
                    break;
            }
        };


        this.timeRangeSelectedCallBack = function(start, end, data) {

            var range = {};
            range.start = start;
            range.end = end;

            this.callBack2('TimeRangeSelected', data, range);
        };

        this.timeRangeSelectedDispatch = function(start, end) {
            switch (calendar.timeRangeSelectedHandling) {
                case 'CallBack':
                    calendar.timeRangeSelectedCallBack(start, end);
                    break;
                case 'JavaScript':
                    calendar.onTimeRangeSelected(start, end);
                    break;
            }
        };

        this.clearSelection = function() {
            calendar.clearShadow();
        };

        this.commandCallBack = function(command, data) {
            this.stopAutoRefresh();

            var params = {};
            params.command = command;

            this.callBack2('Command', data, params);
        };

        this.debug = function(msg, append) {
            if (!this.debuggingEnabled) {
                return;
            }

            if (!calendar.debugMessages) {
                calendar.debugMessages = [];
            }
            calendar.debugMessages.push(msg);

            if (typeof console !== 'undefined') {
                console.log(msg);
            }
        };

        this.registerGlobalHandlers = function() {
            if (!DayPilotMonth.globalHandlers) {
                DayPilotMonth.globalHandlers = true;
                DayPilot.re(document, 'mouseup', DayPilotMonth.gMouseUp);
            }
        };

        this.loadFromServer = function() {
            return (typeof this.events === 'undefined') || (this.events === null);
        };

        this.show = function() {
            if (this.nav.top.style.visibility == 'hidden') {
                this.nav.top.style.visibility = 'visible';
            }
        };

        this.initShort = function() {

            this.prepareRows();
            this.drawTop();
            this.drawTable();
            this.registerGlobalHandlers();
            //this.startAutoRefresh();
            this.callBack2('Init'); // load events
        };

        this.Init = function() {
            var loadFromServer = this.loadFromServer();

            if (loadFromServer) {
                this.initShort();
                return;
            }

            this.prepareRows();
            this.prepareEvents();
            this.drawTop();
            this.drawTable();
            this.show();
            this.drawEvents();

            this.registerGlobalHandlers();

            if (this.messageHTML) {
                this.message(this.messageHTML);
            }

            this.fireAfterRenderDetached(null, false);

        };
    };

    DayPilotMonth.Event = function(calendar, eventPart, row) {
        this.root = calendar;
        this.calendar = calendar;
        this.eventPart = eventPart;

        var ev = this;

        this.value = function() { return eventPart.event.Value; };
        this.text = function() { return eventPart.event.Text; };
        this.start = function() { return new DayPilot.Date(eventPart.event.Start); };
        this.end = function() { return new DayPilot.Date(eventPart.event.End); };

        this.partStart = function() { return calendar.rows[row].getPartStart(eventPart.event); };
        this.partEnd = function() { return calendar.rows[row].getPartEnd(eventPart.event); };

        this.data = {};

        this.clickingAllowed = function() { return true; };

        this.client = {};

        this.toJSON = function(key) {
            var json = {};
            json.value = this.value();
            json.text = this.text();
            json.start = this.start();
            json.end = this.end();
            json.resource = null;
            json.isAllDay = false;
            json.tag = {};

            return json;
        };


    };

    DayPilotMonth.gMouseUp = function(ev) {

        if (DayPilotMonth.movingEvent) {
            var src = DayPilotMonth.movingEvent;

            if (!src.event) {
                return;
            }
            if (!src.event.root) {
                return;
            }
            if (!src.event.root.shadow) {
                return;
            }
            if (!src.event.root.shadow.start) {
                return;
            }

            // load ref
            var calendar = DayPilotMonth.movingEvent.event.root;
            var e = DayPilotMonth.movingEvent.event;
            var start = calendar.shadow.start;
            var position = calendar.shadow.position;
            var offset = DayPilotMonth.movingEvent.offset;

            // cleanup
            calendar.clearShadow();
            DayPilotMonth.movingEvent = null;

            var ev = ev || window.event;

            // fire the event
            calendar.eventMoveDispatch(e, start.x, start.y, offset, ev, position);

            ev.cancelBubble = true;
            if (ev.stopPropagation) {
                ev.stopPropagation();
            }
            DayPilotMonth.movingEvent = null;
            return false;
        }
        else if (DayPilotMonth.resizingEvent) {
            var src = DayPilotMonth.resizingEvent;

            if (!src.event) {
                return;
            }
            if (!src.event.root) {
                return;
            }
            if (!src.event.root.shadow) {
                return;
            }
            if (!src.event.root.shadow.start) {
                return;
            }

            // load ref
            var calendar = DayPilotMonth.resizingEvent.event.root;

            var e = DayPilotMonth.resizingEvent.event;
            var start = calendar.shadow.start;
            var width = calendar.shadow.width;

            // cleanup
            calendar.clearShadow();
            DayPilotMonth.resizingEvent = null;

            // fire the event
            calendar.eventResizeDispatch(e, start, width);

            ev.cancelBubble = true;
            DayPilotMonth.resizingEvent = null;
            return false;
        }
        else if (DayPilotMonth.timeRangeSelecting) {
            if (DayPilotMonth.timeRangeSelecting.moved) {
                var sel = DayPilotMonth.timeRangeSelecting;
                var calendar = sel.root;

                var start = new DayPilot.Date(calendar.getDateFromCell(sel.from.x, sel.from.y));
                var end = start.addDays(sel.width);
                calendar.timeRangeSelectedDispatch(start, end);

                calendar.clearShadow();
            }
            DayPilotMonth.timeRangeSelecting = null;
        }
    };

    // publish the API 

    // (backwards compatibility)    
    //DayPilot.MonthVisible.dragStart = DayPilotMonth.dragStart;
    DayPilot.MonthVisible.Month = DayPilotMonth.Month;

    // current
    DayPilot.Month = DayPilotMonth.Month;

    // experimental jQuery bindings
    if (typeof jQuery !== 'undefined') {
        (function($) {
            $.fn.daypilotMonth = function(options) {
                var first = null;
                var j = this.each(function() {
                    if (this.daypilot) { // already initialized
                        return;
                    };

                    var daypilot = new DayPilot.Month(this.id);
                    this.daypilot = daypilot;
                    for (name in options) {
                        daypilot[name] = options[name];
                    }
                    daypilot.Init();
                    if (!first) {
                        first = daypilot;
                    }
                });
                if (this.length === 1) {
                    return first;
                }
                else {
                    return j;
                }
            };
        })(jQuery);
    }

    if (typeof Sys !== 'undefined' && Sys.Application && Sys.Application.notifyScriptLoaded) {
        Sys.Application.notifyScriptLoaded();
    }


})();