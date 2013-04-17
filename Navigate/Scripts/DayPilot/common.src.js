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

(function() {

    if (typeof DayPilot.$ !== 'undefined') {
        return;
    }

    DayPilot.$ = function(id) {
      return document.getElementById(id);    
    };

    DayPilot.isKhtml = (navigator && navigator.userAgent && navigator.userAgent.indexOf("KHTML") != -1);
    DayPilot.isIE = (navigator && navigator.userAgent && navigator.userAgent.indexOf("MSIE") != -1);

    DayPilot.mo2 = function (target, ev){
        ev = ev || window.event;

        // IE
        if (typeof(ev.offsetX) !== 'undefined') {
        
            var coords = {x: ev.offsetX + 1, y:ev.offsetY  + 1};
            //document.title = "ie/doc:" + document.documentElement.scrollTop;
            
            if (!target) {
                return coords;
            }
            
            var current = ev.srcElement;
            while (current && current != target) {
                if (current.tagName != 'SPAN') { // hack for DayPilotMonth/IE, hour info on the right side of an event
		            coords.x += current.offsetLeft;
		            if (current.offsetTop > 0) {  // hack for http://forums.daypilot.org/Topic.aspx/879/move_event_bug
		                coords.y += current.offsetTop - current.scrollTop;
		            }
		        }

		        current = current.offsetParent;
	        }
    	    
	        if (current) {
		        return coords;	
	        }
	        return null;
        }

        // FF
        if (typeof(ev.layerX) !== 'undefined') {
            
            var coords = {x:ev.layerX, y:ev.layerY, src: ev.target};

            if (!target) {
                return coords;
            }
	        var current = ev.target;
    	    
	        // find the positioned offsetParent, the layerX reference
	        while (current && current.style.position != 'absolute' && current.style.position != 'relative') {
	            current = current.parentNode;
	            if (DayPilot.isKhtml) { // hack for KHTML (Safari and Google Chrome), used in DPC/event moving
	                coords.y += current.scrollTop;
	            }
	        }
    	    
	        while (current && current != target) {
		        coords.x += current.offsetLeft;
		        coords.y += current.offsetTop - current.scrollTop;
		        current = current.offsetParent;
	        }
	        if (current) {
		        return coords;	
	        }
    	    
	        return null;
        }
        
        return null;
    };

    // mouse offset relative to the specified target
    DayPilot.mo3 = function (target, ev, noscroll){
        ev = ev || window.event;

        if(typeof(ev.pageX) !== 'undefined') {
            var abs = DayPilot.abs(target, noscroll);
            var coords = { x: ev.pageX - abs.x, y: ev.pageY - abs.y };
            return coords;
        }
        
        return DayPilot.mo2(target, ev);

    };
    
    // absolute element position on page
    DayPilot.abs = function (element, visible) {
        if (!element) {
            return null;
        }
        
        var r = { 
            x: element.offsetLeft, 
            y: element.offsetTop,
            w: element.clientWidth,
            h: element.clientHeight,
            toString: function() {
                return "x:" + this.x + " y:" + this.y + " w:" + this.w + " h:" + this.h;
            }
        };
        
        if (element.getBoundingClientRect) {
            var b = element.getBoundingClientRect();
            r.x = b.left;
            r.y = b.top;
            
            var d = DayPilot.doc();
            r.x -= d.clientLeft || 0;
            r.y -= d.clientTop || 0;

            var pageOffset = DayPilot.pageOffset();
            r.x += pageOffset.x;
            r.y += pageOffset.y;
            
            if (visible) {
                // use diff, absOffsetBased is not as accurate
                var full = DayPilot.absOffsetBased(element, false);
                var visible = DayPilot.absOffsetBased(element, true);
                
                r.x += visible.x - full.x;
                r.y += visible.y - full.y;
                r.w = visible.w;
                r.h = visible.h;
            }
            
            return r;
        }
        else {
            return DayPilot.absOffsetBased(element, visible);
        }
        
    };

    // old implementation of absolute position
    // problems with adjacent float and margin-left in IE7
    // still the best way to calculate the visible part of the element
    DayPilot.absOffsetBased = function(element, visible) {
        var r = { 
            x: element.offsetLeft, 
            y: element.offsetTop,
            w: element.clientWidth,
            h: element.clientHeight,
            toString: function() {
                return "x:" + this.x + " y:" + this.y + " w:" + this.w + " h:" + this.h;
            }
        };
        
        while (element.offsetParent) {
            element = element.offsetParent;   
            
            r.x -= element.scrollLeft;
            r.y -= element.scrollTop;

            if (visible) {  // calculates the visible part
                if (r.x < 0) {
                    r.w += r.x; // decrease width
                    r.x = 0;
                }

                if (r.y < 0) {
                    r.h += r.y; // decrease height
                    r.y = 0;
                }

                if (element.scrollLeft > 0 && r.x + r.w > element.clientWidth) {
                    r.w -= r.x + r.w - element.clientWidth;
                }
                
                if (element.scrollTop && r.y + r.h > element.clientHeight) {
                    r.h -= r.y + r.h - element.clientHeight;
                }
            }
            
            r.x += element.offsetLeft;
            r.y += element.offsetTop;
            
        }
        
        var pageOffset = DayPilot.pageOffset();
        r.x += pageOffset.x;
        r.y += pageOffset.y;
        
        return r;
    };
    
    // document element
    DayPilot.doc = function() {
        var de = document.documentElement;
        return (de && de.clientHeight) ? de : document.body;
    };
    
    
    DayPilot.pageOffset = function() {
        if (typeof pageXOffset !== 'undefined') {
            return { x: pageXOffset, y: pageYOffset };
        }
        var d = DayPilot.doc();
        return { x: d.scrollLeft, y: d.scrollTop };
    };
	
    DayPilot.indexOf = function(array, object) {
        if (!array || !array.length) {
            return -1;
        }
        for (var i = 0; i < array.length; i++) {
            if (array[i] === object) {
                return i;
            }
        }
        return -1;
    };

    // mouse coords
    DayPilot.mc = function(ev){
        if(ev.pageX || ev.pageY){
	        return {x:ev.pageX, y:ev.pageY};
        }
        return {
	        x:ev.clientX + document.documentElement.scrollLeft,
	        y:ev.clientY + document.documentElement.scrollTop
        };
    };
    // register event
    DayPilot.re = function (el, ev, func) {
        if (el.addEventListener) {
            el.addEventListener (ev, func, false);
        } else if (el.attachEvent) {
            el.attachEvent ("on" + ev, func);
        } 
    };
    
    // purge
    // thanks to http://javascript.crockford.com/memory/leak.html
    DayPilot.pu = function(d) {
        //var removed = [];
        //var start = new Date();
        var a = d.attributes, i, l, n;
        if (a) {
            l = a.length;
            for (i = 0; i < l; i += 1) {
                if (!a[i]) {
                    continue;
                }
                n = a[i].name;
                if (typeof d[n] === 'function') {
                    d[n] = null;
                }
            }
        }
        a = d.childNodes;
        if (a) {
            l = a.length;
            for (i = 0; i < l; i += 1) {
                var children = DayPilot.pu(d.childNodes[i]);
            }
        }
    };
    
    // delete element
    DayPilot.de = function(e) {
        if (!e) {
            return;
        }
        if (!e.parentNode) {
            return;
        }
        e.parentNode.removeChild(e);
    };
    
    // vertical scrollbar width
    DayPilot.sw = function(element) {
        if (!element) {
            return 0;
        }
        return element.offsetWidth - element.clientWidth - 2;
    };
    
    DayPilot.Selection = function (start, end, resource, root) {
        this.type = 'selection';
        this.start = start.isDayPilotDate ? start : new DayPilot.Date(start);
        this.end = end.isDayPilotDate ? end : new DayPilot.Date(end);
        this.resource = resource;
        this.root = root;
        
        this.toJSON = function(key) {
            var json = {};
            json.start = this.start;
            json.end = this.end;
            json.resource = this.resource;
            
            return json;
        };
    };

    /* XMLHttpRequest */

    DayPilot.request = function (url, callback, postData, errorCallback) {
	    var req = DayPilot.createXmlHttp();
	    if (!req) {
	        return;
	    }
    	
	    req.open("POST",url,true);
        req.setRequestHeader('Content-type','text/plain');
	    req.onreadystatechange = function () {
		    if (req.readyState != 4) return;
		    if (req.status != 200 && req.status != 304) {
		        if (errorCallback) {
		            errorCallback(req);
		        }
		        else {
			        alert('HTTP error ' + req.status);
			    }
			    return;
		    }
		    callback(req);
	    };
	    if (req.readyState == 4) {
	        return;
	    }
	    req.send(postData);
    };

    DayPilot.createXmlHttp = function () {
        var xmlHttp;
        try {
            xmlHttp = new XMLHttpRequest();
        }
        catch(e) {
            try {
                xmlHttp = new ActiveXObject("Microsoft.XMLHTTP");
            }
            catch(e) {
            }
        }
        return xmlHttp;
    };


    /* Date utils */

    // DayPilot.Date class
    /* Constructor signatures:
     
     -- new DayPilot.Date(date, isLocal)
     date - JavaScript Date object
     isLocal - true if the local time should be taken from date, otherwise GMT base is used

     -- new DayPilot.Date() - returns now, using local date
     
     -- new DayPilot.Date(string)
     string - date in "sortable" format, e.g. 2009-01-01T00:00:00
     
     */
    DayPilot.Date = function(date, isLocal) {
        this.isDayPilotDate = true; // allow class detection

        if (typeof date === 'undefined') {  // date not set, use NOW
            this.d = DayPilot.Date.fromLocal();    
        }
        else if (typeof date === "string") {
            return DayPilot.Date.fromStringSortable(date);
        }
        else if (date.isDayPilotDate) { // it's already DayPilot.Date object, return it (no copy)
            return date;
        }
        else if (!date.getFullYear) {  // it's not a date object, fail
            throw "date parameter is not a Date object: " + date;
        }
        else if (isLocal) {  // if the date passed should be read as local date
            this.d = DayPilot.Date.fromLocal(date);
        }
        else {  // should be read as GMT
            this.d = date;
        }
        
        this.ticks = this.d.getTime();
        
    };

    DayPilot.Date.prototype.addDays = function(days) {
        return new DayPilot.Date(DayPilot.Date.addDays(this.d, days));
    };

    DayPilot.Date.prototype.addHours = function(hours) {
        return this.addTime(hours*60*60*1000);
    };

    DayPilot.Date.prototype.addMilliseconds = function(millis) {
        return this.addTime(millis);
    };

    DayPilot.Date.prototype.addMinutes = function(minutes) {
        return this.addTime(minutes*60*1000);
    };

    DayPilot.Date.prototype.addMonths = function(months) {
        return new DayPilot.Date(DayPilot.Date.addMonths(this.d, months));
    };

    DayPilot.Date.prototype.addSeconds = function(seconds) {
        return this.addTime(seconds*1000);
    };

    DayPilot.Date.prototype.addTime = function(ticks) {
        return new DayPilot.Date(DayPilot.Date.addTime(this.d, ticks));
    };

    DayPilot.Date.prototype.addYears = function(years) {
        var n = this.clone();
        n.d.setUTCFullYear(this.getYear() + years);
        return n;
    };

    DayPilot.Date.prototype.clone = function() {
        return new DayPilot.Date(DayPilot.Date.clone(this.d));
    };

    DayPilot.Date.prototype.dayOfWeek = function() {
        return this.d.getUTCDay();
    };
    
    DayPilot.Date.prototype.daysInMonth = function() {
        return DayPilot.Date.daysInMonth(this.d);
    };

    DayPilot.Date.prototype.dayOfYear = function() {
    	return Math.ceil((this.getDatePart().getTime() - this.firstDayOfYear().getTime()) / 86400000) + 1;
    };

    DayPilot.Date.prototype.equals = function(another) {
        if (another === null) {
            return false;
        }
        if (another.isDayPilotDate) {
            return DayPilot.Date.equals(this.d, another.d);
        }
        else {
            throw "The parameter must be a DayPilot.Date object (DayPilot.Date.equals())";
        }
    };
    
    DayPilot.Date.prototype.firstDayOfMonth = function() {
        var utc = DayPilot.Date.firstDayOfMonth(this.getYear(), this.getMonth() + 1);
        return new DayPilot.Date(utc);
    };

    DayPilot.Date.prototype.firstDayOfYear = function() {
        var year = this.getYear();
        var d = new Date();
        d.setUTCFullYear(year, 0, 1);
        d.setUTCHours(0);
        d.setUTCMinutes(0);
        d.setUTCSeconds(0);
        d.setUTCMilliseconds(0);
        return new DayPilot.Date(d);
    };

    DayPilot.Date.prototype.firstDayOfWeek = function(weekStarts) {
        var utc = DayPilot.Date.firstDayOfWeek(this.d, weekStarts);
        return new DayPilot.Date(utc);
    };

    DayPilot.Date.prototype.getDay = function() {
        return this.d.getUTCDate();
    };

    DayPilot.Date.prototype.getDatePart = function() {
        return new DayPilot.Date(DayPilot.Date.getDate(this.d));
    };

    DayPilot.Date.prototype.getYear = function() {
        return this.d.getUTCFullYear();
    };

    DayPilot.Date.prototype.getHours = function() {
        return this.d.getUTCHours();
    };

    DayPilot.Date.prototype.getMilliseconds = function() {
        return this.d.getUTCMilliseconds();
    };

    DayPilot.Date.prototype.getMinutes = function() {
        return this.d.getUTCMinutes();
    };

    DayPilot.Date.prototype.getMonth = function() {
        return this.d.getUTCMonth();
    };

    DayPilot.Date.prototype.getSeconds = function() {
        return this.d.getUTCSeconds();
    };

    DayPilot.Date.prototype.getTotalTicks = function() {
        return this.getTime();
    };

    // undocumented
    DayPilot.Date.prototype.getTime = function() {
        if (typeof this.ticks !== 'number') {
            throw "Uninitialized DayPilot.Date (internal error)";
        }
        return this.ticks;
    };

    DayPilot.Date.prototype.getTimePart = function() {
        return DayPilot.Date.getTime(this.d);
    };

    DayPilot.Date.prototype.lastDayOfMonth = function() {
        var utc = DayPilot.Date.lastDayOfMonth(this.getYear(), this.getMonth() + 1);
        return new DayPilot.Date(utc);
    };
    
    DayPilot.Date.prototype.weekNumber = function() {
        var first = this.firstDayOfYear();
        var days = (this.getTime() - first.getTime()) / 86400000;
        return Math.ceil((days + first.dayOfWeek() + 1) / 7);
    };
    
    // ISO 8601
    DayPilot.Date.prototype.weekNumberISO = function() {
        var thursdayFlag = false;
        var dayOfYear = this.dayOfYear();

        var startWeekDayOfYear = this.firstDayOfYear().dayOfWeek();
        var endWeekDayOfYear = this.firstDayOfYear().addYears(1).addDays(-1).dayOfWeek();
        //int startWeekDayOfYear = new DateTime(date.getYear(), 1, 1).getDayOfWeekOrdinal();
        //int endWeekDayOfYear = new DateTime(date.getYear(), 12, 31).getDayOfWeekOrdinal();

        if (startWeekDayOfYear == 0) {
            startWeekDayOfYear = 7;
        }
        if (endWeekDayOfYear == 0) {
            endWeekDayOfYear = 7;
        }

        var daysInFirstWeek = 8 - (startWeekDayOfYear);

        if (startWeekDayOfYear == 4 || endWeekDayOfYear == 4) {
            thursdayFlag = true;
        }

        var fullWeeks = Math.ceil((dayOfYear - (daysInFirstWeek)) / 7.0);

        var weekNumber = fullWeeks;

        if (daysInFirstWeek >= 4) {
            weekNumber = weekNumber + 1;
        }

        if (weekNumber > 52 && !thursdayFlag) {
            weekNumber = 1;
        }

        if (weekNumber == 0) {
            weekNumber = this.firstDayOfYear().addDays(-1).weekNumberISO();//weekNrISO8601(new DateTime(date.getYear() - 1, 12, 31));
        }
        
        return weekNumber;
    
    };
	
	DayPilot.Date.prototype.toDateLocal = function() {
		return DayPilot.Date.toLocal(this.d);
	};

    DayPilot.Date.prototype.toJSON = function() {
        return this.toStringSortable();
    };

    // formatting and languages needed here
    DayPilot.Date.prototype.toString = function() {
        //return DayPilot.Date.toLocal(this.d).toLocaleString();
        return this.toStringSortable();
    };

    DayPilot.Date.prototype.toStringSortable = function() {
        return DayPilot.Date.toStringSortable(this.d);
    };

    /* static functions, return DayPilot.Date object */
    DayPilot.Date.fromStringSortable = function(string) {
        var datetime = /^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2})$/;
        var date = /^(\d{4})-(\d{2})-(\d{2})$/;
        
        var isValidDateTime = datetime.test(string);
        var isValidDate = date.test(string);
        var isValid = isValidDateTime || isValidDate;
        
        if (!isValid) {
            throw "Invalid string format (use '2010-01-01' or '2010-01-01T00:00:00'.";
        }
        
        var regex = isValidDateTime ? datetime : date;
        
        var m = regex.exec(string);
        
        //return m[1];
        
        var d = new Date();
        d.setUTCFullYear(m[1], m[2] - 1, m[3]);
        d.setUTCHours(m[4] ? m[4] : 0);
        d.setUTCMinutes(m[5] ? m[5] : 0);
        d.setUTCSeconds(m[6] ? m[6] : 0);
        d.setUTCMilliseconds(0);
        
        return new DayPilot.Date(d);
    };
    
    /* internal functions, all operate with GMT base of the date object 
      (except of DayPilot.Date.fromLocal()) */

    DayPilot.Date.addDays = function(date, days) {
        var d = new Date();
        d.setTime(date.getTime() + days * 24 * 60 *60 * 1000);
        return d;    
    };

    DayPilot.Date.addMinutes = function(date, minutes) {
        var d = new Date();
        d.setTime(date.getTime() + minutes * 60 * 1000);
        return d;    
    };

    DayPilot.Date.addMonths = function(date, months) {
        if (months == 0)
            return date;
        
        var y = date.getUTCFullYear();
        var m = date.getUTCMonth() + 1;
        
        if (months > 0) {
            while (months >= 12) {
                months -= 12;
                y++;
            }
            if (months > 12 - m) {
                y++;
                m = months - (12 - m);
            } 
            else {
                m += months;
            }
        }
        else {
            while (months <= -12) {
                months += 12;
                y--;
            }
            if (m <= months) {  // 
                y--;
                m = 12 - (months + m);
            }
            else {
                m = m + months;
            }
        }
        
        var d = DayPilot.Date.clone(date);
        d.setUTCFullYear(y);
        d.setUTCMonth(m - 1);
        
        return d;
    };

    DayPilot.Date.addTime = function(date, time) {
        var d = new Date();
        d.setTime(date.getTime() + time);
        return d;    
    };

    DayPilot.Date.clone = function (original) {
        var d = new Date();
        return DayPilot.Date.dateFromTicks(original.getTime());
    };


    // rename candidate: diffDays
    DayPilot.Date.daysDiff = function(first, second) {
        if (first.getTime() > second.getTime()) {
            return null;
        }
        
        var i = 0;
        var fDay = DayPilot.Date.getDate(first);
        var sDay = DayPilot.Date.getDate(second);
        
        while (fDay < sDay) {
            fDay = DayPilot.Date.addDays(fDay, 1);
            i++;
        }
        
        return i;
    };

    DayPilot.Date.daysInMonth = function(year, month) {  // accepts also: function(date)
        if (year.getUTCFullYear) { // it's a date object
            month = year.getUTCMonth() + 1;
            year = year.getUTCFullYear();
        }

        var m = [31,28,31,30,31,30,31,31,30,31,30,31];
        if (month != 2) return m[month - 1];
        if (year%4 != 0) return m[1];
        if (year%100 == 0 && year%400 != 0) return m[1];
        return m[1] + 1;
    };

    DayPilot.Date.daysSpan = function(first, second) {
        if (first.getTime() == second.getTime()) {
            return 0;
        }

        var diff = DayPilot.Date.daysDiff(first, second);
        
        if (DayPilot.Date.equals(second, DayPilot.Date.getDate(second))) {
            diff--;
        }
        
        return diff;
    };

    DayPilot.Date.diff = function(first, second) { // = first - second
        if (!(first && second && first.getTime && second.getTime)) {
            throw "Both compared objects must be Date objects (DayPilot.Date.diff).";
        }
        
        return first.getTime() - second.getTime();
    };

    DayPilot.Date.equals = function (first, second) {
        return first.getTime() == second.getTime();
    };

    DayPilot.Date.fromLocal = function(localDate) {
        if (!localDate) {
            localDate = new Date();
        }

        var d = new Date();
        d.setUTCFullYear(localDate.getFullYear(), localDate.getMonth(), localDate.getDate());
        d.setUTCHours(localDate.getHours());
        d.setUTCMinutes(localDate.getMinutes());
        d.setUTCSeconds(localDate.getSeconds());
        d.setUTCMilliseconds(localDate.getMilliseconds());
        return d;
    };

    DayPilot.Date.firstDayOfMonth = function(year, month) {
        var d = new Date();
        d.setUTCFullYear(year, month -1, 1);
        d.setUTCHours(0);
        d.setUTCMinutes(0);
        d.setUTCSeconds(0);
        d.setUTCMilliseconds(0);
        return d;
    };

    DayPilot.Date.firstDayOfWeek = function(d, weekStarts) {
        var day = d.getUTCDay();
        while (day != weekStarts) {
            d = DayPilot.Date.addDays(d, -1);
            day = d.getUTCDay();
        }
        return d;
    };


    // rename candidate: fromTicks
    DayPilot.Date.dateFromTicks = function (ticks) {
        var d = new Date();
        d.setTime(ticks);
        return d;
    };

    // rename candidate: getDatePart
    DayPilot.Date.getDate = function (original) {
        var d = DayPilot.Date.clone(original);
        d.setUTCHours(0);
        d.setUTCMinutes(0);
        d.setUTCSeconds(0);
        d.setUTCMilliseconds(0);
        return d;    
    };

    DayPilot.Date.getStart = function(year, month, weekStarts) {  // gets the first days of week where the first day of month occurs
        var fdom = DayPilot.Date.firstDayOfMonth(year, month);
        d = DayPilot.Date.firstDayOfWeek(fdom, weekStarts);
        return d;
    };

    // rename candidate: getTimePart
    DayPilot.Date.getTime = function (original) {
        var date = DayPilot.Date.getDate(original);
        
        return DayPilot.Date.diff(original, date);
    };

    // rename candidate: toHourString
    DayPilot.Date.hours = function(date, use12) {

        var minute = date.getUTCMinutes();
        if (minute < 10) minute = "0" + minute;


        var hour = date.getUTCHours();
        //if (hour < 10) hour = "0" + hour;
        
        if (use12) {
            var am = hour < 12;
            var hour = hour % 12;
            if (hour == 0) {
                hour = 12;
            }
            var suffix = am ? "AM" : "PM";
            return hour + ':' + minute + ' ' + suffix;
        }
        else {
            return hour + ':' + minute;
        }
    };

    DayPilot.Date.lastDayOfMonth = function(year, month) {
        var d = DayPilot.Date.firstDayOfMonth(year, month);
        var length = DayPilot.Date.daysInMonth(year, month);
        d.setUTCDate(length);
        return d;
    };

    DayPilot.Date.max = function (first, second) {
        if (first.getTime() > second.getTime()) {
            return first;
        }
        else {
            return second;
        }
    };

    DayPilot.Date.min = function (first, second) {
        if (first.getTime() < second.getTime()) {
            return first;
        }
        else {
            return second;
        }
    };

    DayPilot.Date.today = function() {
        var relative = new Date();
        var d = new Date();
        d.setUTCFullYear(relative.getFullYear());
        d.setUTCMonth(relative.getMonth());
        d.setUTCDate(relative.getDate());
        
        return d;
    };

    DayPilot.Date.toLocal = function(date) {
        if (!date) {
            date = new Date();
        }

        var d = new Date();
        d.setFullYear(date.getUTCFullYear(), date.getUTCMonth(), date.getUTCDate());
        d.setHours(date.getUTCHours());
        d.setMinutes(date.getUTCMinutes());
        d.setSeconds(date.getUTCSeconds());
        d.setMilliseconds(date.getUTCMilliseconds());
        return d;
    };


    DayPilot.Date.toStringSortable = function(date) {
        if (date.isDayPilotDate) {
            return date.toStringSortable();
        }

        var d = date;
        var second = d.getUTCSeconds();
        if (second < 10) second = "0" + second;
        var minute = d.getUTCMinutes();
        if (minute < 10) minute = "0" + minute;
        var hour = d.getUTCHours();
        if (hour < 10) hour = "0" + hour;
        var day = d.getUTCDate();
        if (day < 10) day = "0" + day;
        var month = d.getUTCMonth() + 1;
        if (month < 10) month = "0" + month;
        var year = d.getUTCFullYear();
        
        if (year <= 0) {
            throw "The minimum year supported is 1.";
        }
        if (year < 10) {
            year = "000" + year;
        }
        else if (year < 100) {
            year = "00" + year;
        }
        else if (year < 1000) {
            year = "0" + year;
        }
        
        return year + "-" + month + "-" + day + 'T' + hour + ":" + minute + ":" + second;
    };

})();

/* JSON */
// thanks to http://www.json.org/js.html


// declares DayPilot.JSON.stringify()
DayPilot.JSON = {};

(function () {
    function f(n) {
        return n < 10 ? '0' + n : n;
    }

    if (typeof Date.prototype.toJSON2 !== 'function') {

        Date.prototype.toJSON2 = function (key) {
            return this.getUTCFullYear()   + '-' +
                         f(this.getUTCMonth() + 1) + '-' +
                         f(this.getUTCDate())      + 'T' +
                         f(this.getUTCHours())     + ':' +
                         f(this.getUTCMinutes())   + ':' +
                         f(this.getUTCSeconds())   + '';
        };

        String.prototype.toJSON =
        Number.prototype.toJSON =
        Boolean.prototype.toJSON = function (key) {
            return this.valueOf();
        };
    }

    var cx = /[\u0000\u00ad\u0600-\u0604\u070f\u17b4\u17b5\u200c-\u200f\u2028-\u202f\u2060-\u206f\ufeff\ufff0-\uffff]/g,
        escapeable = /[\\\"\x00-\x1f\x7f-\x9f\u00ad\u0600-\u0604\u070f\u17b4\u17b5\u200c-\u200f\u2028-\u202f\u2060-\u206f\ufeff\ufff0-\uffff]/g,
        gap,
        indent,
        meta = {    
            '\b': '\\b',
            '\t': '\\t',
            '\n': '\\n',
            '\f': '\\f',
            '\r': '\\r',
            '"' : '\\"',
            '\\': '\\\\'
        },
        rep;

    function quote(string) {
        escapeable.lastIndex = 0;
        return escapeable.test(string) ?
            '"' + string.replace(escapeable, function (a) {
                var c = meta[a];
                if (typeof c === 'string') {
                    return c;
                }
                return '\\u' + ('0000' + a.charCodeAt(0).toString(16)).slice(-4);
            }) + '"' :
            '"' + string + '"';
    }

    function str(key, holder) {
        var i,          
            k,          
            v,          
            length,
            mind = gap,
            partial,
            value = holder[key];
        if (value && typeof value === 'object' && typeof value.toJSON2 === 'function') {
            value = value.toJSON2(key);
        }
        else if (value && typeof value === 'object' && typeof value.toJSON === 'function' && !value.ignoreToJSON) {
            value = value.toJSON(key);
        }
        if (typeof rep === 'function') {
            value = rep.call(holder, key, value);
        }
        switch (typeof value) {
        case 'string':
            return quote(value);
        case 'number':
            return isFinite(value) ? String(value) : 'null';
        case 'boolean':
        case 'null':
            return String(value);
        case 'object':
            if (!value) {
                return 'null';
            }
            gap += indent;
            partial = [];
            if (typeof value.length === 'number' &&
                    !value.propertyIsEnumerable('length')) {
                length = value.length;
                for (i = 0; i < length; i += 1) {
                    partial[i] = str(i, value) || 'null';
                }
                v = partial.length === 0 ? '[]' :
                    gap ? '[\n' + gap +
                            partial.join(',\n' + gap) + '\n' +
                                mind + ']' :
                          '[' + partial.join(',') + ']';
                gap = mind;
                return v;
            }
            if (rep && typeof rep === 'object') {
                length = rep.length;
                for (i = 0; i < length; i += 1) {
                    k = rep[i];
                    if (typeof k === 'string') {
                        v = str(k, value);
                        if (v) {
                            partial.push(quote(k) + (gap ? ': ' : ':') + v);
                        }
                    }
                }
            } else {
                for (k in value) {
                    if (Object.hasOwnProperty.call(value, k)) {
                        v = str(k, value);
                        if (v) {
                            partial.push(quote(k) + (gap ? ': ' : ':') + v);
                        }
                    }
                }
            }
            v = (partial.length === 0) ? '{\u007D' :
                gap ? '{\n' + gap + partial.join(',\n' + gap) + '\n' +
                        mind + '\u007D' : '{' + partial.join(',') + '\u007D';
            gap = mind;
            return v;
        }
    }

    if (typeof DayPilot.JSON.stringify !== 'function') {
        DayPilot.JSON.stringify = function (value, replacer, space) {
            var i;
            gap = '';
            indent = '';
            if (typeof space === 'number') {
                for (i = 0; i < space; i += 1) {
                    indent += ' ';
                }
            } else if (typeof space === 'string') {
                indent = space;
            }
            rep = replacer;
            if (replacer && typeof replacer !== 'function' && (typeof replacer !== 'object' || typeof replacer.length !== 'number')) {
                throw new Error('JSON.stringify');
            }
            return str('', {'': value});
        };
    }

})();
