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

    var doNothing = function() {};

    if (typeof DayPilot.Calendar !== 'undefined') {
        return;
    }

    var DayPilotCalendar = {};
    
    // internal selecting
    DayPilotCalendar.selectedCells = null;
    DayPilotCalendar.topSelectedCell = null;
    DayPilotCalendar.bottomSelectedCell = null;
    DayPilotCalendar.selecting = false;
    DayPilotCalendar.column = null;
    DayPilotCalendar.firstSelected = null;
    DayPilotCalendar.firstMousePos = null;
        
    // internal resizing
    DayPilotCalendar.originalMouse = null;
    DayPilotCalendar.originalHeight = null;
    DayPilotCalendar.originalTop = null;
    DayPilotCalendar.resizing = null;
    DayPilotCalendar.globalHandlers = false; 
        
    // internal moving
    DayPilotCalendar.moving = null;

    // helpers
    DayPilotCalendar.register = function(calendar) {
        if (!DayPilotCalendar.registered) {
            DayPilotCalendar.registered = [];
        }
        var r = DayPilotCalendar.registered;
        
        for (var i = 0; i < r.length; i++) {
            if (r[i] === calendar) {
                return;
            }
        }
        r.push(calendar);
    };

    DayPilotCalendar.unregister = function (calendar) {
        var a = DayPilotCalendar.registered;
        if (!a) {
            return;
        }
        
        var i = DayPilot.indexOf(a, calendar);
        if (i === -1) {
            return;
        }
        a.splice(i, 1);
    };

    DayPilotCalendar.getCellsAbove = function(cell) {
        var array = [];
        var c = DayPilotCalendar.getColumn(cell);

        var tr = cell.parentNode;
        
        var select = null;
        while (tr && select !== DayPilotCalendar.firstSelected) {
            select = tr.getElementsByTagName("td")[c];
            array.push(select);
            tr = tr.previousSibling;
            while (tr && tr.tagName !== "TR") {
                tr = tr.previousSibling;
	        }
        }
        return array;
    };
        
    DayPilotCalendar.getCellsBelow = function(cell) {
        var array = [];
        var c = DayPilotCalendar.getColumn(cell);
        var tr = cell.parentNode;
        
        var select = null;
        while (tr && select !== DayPilotCalendar.firstSelected) {
            select = tr.getElementsByTagName("td")[c];
            array.push(select);
            tr = tr.nextSibling;
            while (tr && tr.tagName !== "TR") {
                tr = tr.nextSibling;
	        }
        }
        return array;
    };

    DayPilotCalendar.getColumn = function(cell) {
        var i = 0;
        while (cell.previousSibling) {
            cell = cell.previousSibling;
            if (cell.tagName === "TD") {
                i++;
	    }
        }
        return i;
    };
        
    DayPilotCalendar.gUnload = function (ev) {
        
        if (!DayPilotCalendar.registered) {
            return;
        }
        var r = DayPilotCalendar.registered;
        
        for (var i = 0; i < r.length; i++) {
            var c = r[i];
            c.dispose();
                        
            DayPilotCalendar.unregister(c);
        }
        
    };

    DayPilotCalendar.gMouseUp = function (e){

        if (DayPilotCalendar.resizing) {
            if (!DayPilotCalendar.resizingShadow) {
	            DayPilotCalendar.resizing.style.cursor = 'default';
	            document.body.style.cursor = 'default';
                DayPilotCalendar.resizing = null;     
                return;
            }
        
            var dpEvent = DayPilotCalendar.resizing.event;
            var height = DayPilotCalendar.resizingShadow.clientHeight + 4;
            var top = DayPilotCalendar.resizingShadow.offsetTop;
            var border = DayPilotCalendar.resizing.dpBorder;
            
            // stop resizing on the client
            DayPilotCalendar.deleteShadow(DayPilotCalendar.resizingShadow);
            DayPilotCalendar.resizingShadow = null;
	        DayPilotCalendar.resizing.style.cursor = 'default';
	        document.body.style.cursor = 'default';
            DayPilotCalendar.resizing = null;     
            
            dpEvent.root.eventResizeDispatch(dpEvent, height, top, border);
        }
        else if (DayPilotCalendar.moving) {
            if (!DayPilotCalendar.movingShadow) {
                DayPilotCalendar.moving = null;
	            document.body.style.cursor = 'default';
	            return;
            }
        
            var top = DayPilotCalendar.movingShadow.offsetTop;
            
            DayPilotCalendar.deleteShadow(DayPilotCalendar.movingShadow);
        
            var dpEvent = DayPilotCalendar.moving.event; 
            
            var newColumnIndex = DayPilotCalendar.movingShadow.column;
            
            // stop moving on the client     
            DayPilotCalendar.moving = null;
            DayPilotCalendar.movingShadow = null;
	        document.body.style.cursor = 'default';
            
            var ev = e || window.event;
            dpEvent.root.eventMoveDispatch(dpEvent, newColumnIndex, top, ev);
        }
        
    };

    DayPilotCalendar.deleteShadow = function(shadow) {
        if (!shadow) {
            return;
        }
        if (!shadow.parentNode) {
            return;
        }
        
        shadow.parentNode.removeChild(shadow);
    };

    DayPilotCalendar.createShadow = function(object, copyText) {
        var parentTd = object.parentNode;
        while (parentTd && parentTd.tagName !== "TD") {
            parentTd = parentTd.parentNode;
        }
        
        var shadow = document.createElement('div');
        shadow.setAttribute('unselectable', 'on');
        shadow.style.position = 'absolute';
        shadow.style.width = (object.offsetWidth - 4) + 'px';
        shadow.style.height = (object.offsetHeight - 4) + 'px';
        shadow.style.left = (object.offsetLeft) + 'px';
        shadow.style.top = (object.offsetTop) + 'px';
        shadow.style.border = '2px dotted #666666';
        shadow.style.zIndex = 101;
        
        shadow.style.backgroundColor = "#aaaaaa";
        shadow.style.opacity = 0.5;
        shadow.style.filter = "alpha(opacity=50)";
        shadow.style.border = '2px solid #aaaaaa';
        
        if (copyText) {
            shadow.style.overflow = 'hidden';
            shadow.style.fontSize = object.style.fontSize;
            shadow.style.fontFamily = object.style.fontFamily;
            shadow.style.color = object.style.color;
            shadow.innerHTML = object.data.InnerHTML;
        }
        
        shadow.style.MozBorderRadius = "5px";
        shadow.style.webkitBorderRadius = "5px";
        shadow.style.borderRadius = "5px";
        
        parentTd.firstChild.appendChild(shadow);
        
        return shadow;
    };

    DayPilotCalendar.moveShadow = function(column) {
        var shadow = DayPilotCalendar.movingShadow;
        var parent = shadow.parentNode;
        
        parent.style.display = 'none';
        
        shadow.parentNode.removeChild(shadow);
        column.firstChild.appendChild(shadow);
        shadow.style.left = '0px';
        
        parent.style.display = '';

        shadow.style.width = (DayPilotCalendar.movingShadow.parentNode.offsetWidth + 1) + 'px';
    };

    DayPilotCalendar.Calendar = function(id) {
    
        var isConstructor = false;
        if (this instanceof DayPilotCalendar.Calendar && !this.__constructor) {
            isConstructor = true;
            this.__constructor = true;
        }
        
        if (!isConstructor) {
            throw "DayPilot.Calendar() is a constructor and must be called as 'var c = new DayPilot.Calendar(id);'";
        }
    
        var calendar = this;
        this.uniqueID = null;
        
        this.id = id;
        this.clientName = id;
        
        this.cache = {};
        this.cache.pixels = {};
        
        this.elements = {};
        this.elements.events = [];
        
        this.nav = {};

        this.afterRender = function() {};

        // potentially leaking a bit but significantly faster in IE
        this.fasterDispose = true;
        
        this.borderColor = "#CED2CE";
        this.businessBeginsHour = 9;
        this.businessEndsHour = 18;
        this.cellBackColor = "#ffffff";
        this.cellBorderColor = "#DEDFDE";
        this.cellHeight = 20;
        this.columnMarginRight = 5;
        this.cornerBackColor = "#F3F3F9";
        this.days = 1;
        this.eventBackColor = '#638EDE';
        this.eventBorderColor = "#2951A5";
        this.eventFontFamily = 'Tahoma, Arial, Helvetica, sans-serif';
        this.eventFontSize = '8pt';
        this.eventFontColor = "#ffffff";
        this.eventHeaderFontSize = '8pt';
        this.eventHeaderFontFamily = 'Tahoma, Arial, Helvetica, sans-serif';
        this.eventHeaderFontColor = "#ffffff";
        this.eventHeaderHeight = 14;
        this.eventHeaderVisible = true;
        this.headerFontSize = '10pt';
        this.headerFontFamily = 'Tahoma, Arial, Helvetica, sans-serif';
        this.headerFontColor = "#42658C";
        this.headerHeight = 21;
        this.height = 300;
        this.heightSpec = 'BusinessHours';
        this.hideUntilInit = true;
        this.hourHalfBorderColor = "#EBEDEB";
        this.hourBorderColor = "#DEDFDE";
        this.hourFontColor = "#42658C";
        this.hourFontFamily = "Tahoma, Arial, Helvetica, sans-serif";
        this.hourFontSize = "16pt";
        this.hourNameBackColor = "#F3F3F9";
        this.hourNameBorderColor = "#DEDFDE";
        this.hourWidth = 45;
        this.initScrollPos = 0;
        this.loadingLabelText = "Loading...";
        this.loadingLabelVisible = true;
        this.loadingLabelBackColor = "ff0000";
        this.loadingLabelFontColor = "#ffffff";
        this.loadingLabelFontFamily = "Tahoma, Arial, Helvetica, sans-serif";
        this.loadingLabelFontSize = "10pt";
        this.selectedColor = "#316AC5";
        this.showToolTip = true;
        this.startDate = new DayPilot.Date().getDatePart();
        this.timeFormat = 'Clock12Hours';

        this.timeRangeSelectedHandling = 'Disabled';
        this.eventClickHandling = 'Disabled';
        this.eventResizeHandling = 'Disabled';
        this.eventMoveHandling = 'Disabled';
        
        this.onTimeRangeSelected = function(start, end) { alert(start + '\n' + end); };
        this.onEventClick = function(e) { alert(e); };
        this.onEventResize = function(e) { alert(e); };
        this.onEventMove = function(e) { alert(e); };

        this.clearSelection = function() {
            for(var j = 0; j < DayPilotCalendar.selectedCells.length; j++) {
                var cell = DayPilotCalendar.selectedCells[j];
                if (cell) {
                    if (cell.selected) {
                        cell.removeChild(cell.selected);
                        cell.firstChild.style.display = '';
                        cell.selected = null;
                    }
                }
            }
        };
        
        this.ie = (navigator && navigator.userAgent && navigator.userAgent.indexOf("MSIE") !== -1);  // IE
        this.ff = (navigator && navigator.userAgent && navigator.userAgent.indexOf("Firefox") !== -1);
        this.opera105 = (function() {
            if (/Opera[\/\s](\d+\.\d+)/.test(navigator.userAgent)){
                var v = new Number(RegExp.$1);
                return v >= 10.5;
            }
            return false;
        })();
        this.webkit522 = (function() {
            if (/AppleWebKit[\/\s](\d+\.\d+)/.test(navigator.userAgent)){
                var v = new Number(RegExp.$1);
                return v >= 522;
            }
            return false;
        })();
        this.ie9 = (navigator && navigator.userAgent && navigator.userAgent.indexOf("MSIE 9") !== -1);  // IE
        
        this.cleanSelection = this.clearSelection;
        
        this.callBack2 = function(action, data, parameters) {
        
            if (this.callbackTimeout) {
                window.clearTimeout(this.callbackTimeout);
            }
        
            this.callbackTimeout = window.setTimeout(function() {
                calendar.loadingStart();
            }, 100);    
        
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
        
        this.dispose = function() {
            var c = calendar;
            c.deleteEvents();
            
            c.nav.zoom.onmousemove = null;
            c.nav.scroll.root = null;
            
            DayPilot.pu(c.nav.loading);
            
            c.disposeMain();
            c.disposeHeader();
            
            c.nav.select = null;
            c.nav.cornerRight = null;
            c.nav.scrollable = null;
            c.nav.zoom = null;
            c.nav.loading = null;
            c.nav.header = null;
            c.nav.hourTable = null;
            c.nav.scrolltop = null;
            c.nav.scroll = null;
            c.nav.main = null;
            c.nav.message = null;
            c.nav.messageClose = null;
            c.nav.top = null;
            
            DayPilotCalendar.unregister(c);
        };
        
        this.registerDispose = function() {
            var root = document.getElementById(id);
            root.dispose = this.dispose;
        };
        
        this.callBackResponse = function(response) {
            calendar.updateView(response.responseText);
        };

        this.getCallBackHeader = function() {
            var h = {};

            h.control = "dpc";
            h.id = this.id;

            h.days = calendar.days;
            h.startDate = calendar.startDate;
            h.heightSpec = calendar.heightSpec;
            h.businessBeginsHour = calendar.businessBeginsHour;
            h.businessEndsHour = calendar.businessEndsHour;

            h.backColor = calendar.cellBackColor;
            h.timeFormat = calendar.timeFormat;
            h.viewType = calendar.viewType;
            h.locale = calendar.locale;
            
            return h;
        };
        
        
        this.updateView = function(result, context) {
        
            var result = eval("(" + result + ")");

            if (result.UpdateType === "None") {
                calendar.loadingStop();
                calendar.show();
                return;
            }

            calendar.deleteEvents();
            
            if (result.UpdateType === "Full") {
                
                calendar.columns = result.Columns;

                // properties
                calendar.days = result.Days; 
                calendar.startDate = new DayPilot.Date(result.StartDate);
                calendar.heightSpec = result.HeightSpec ? result.HeightSpec : calendar.heightSpec; 
                calendar.businessBeginsHour = result.BusinessBeginsHour ? result.BusinessBeginsHour : calendar.businessBeginsHour;
                calendar.businessEndsHour = result.BusinessEndsHour ? result.BusinessEndsHour : calendar.businessEndsHour;
                calendar.viewType = result.ViewType; //
                calendar.backColor = result.BackColor ? result.BackColor : calendar.backColor;
                calendar.eventHeaderVisible = result.EventHeaderVisible ? result.EventHeaderVisible : calendar.eventHeaderVisible;
                calendar.timeFormat = result.TimeFormat ? result.TimeFormat : calendar.timeFormat;
                calendar.locale = result.Locale ? result.Locale : calendar.locale;
				
                calendar.prepareColumns();
            }            
            
            calendar.loadEvents(result.Events);
            calendar.updateHeaderHeight();
            
            if (result.UpdateType === "Full") {
                calendar.drawHeader();
                calendar.drawMain();
                calendar.drawHourTable();
                calendar.updateHeight();
            }
            
            calendar.show();
			
            calendar.drawEvents();
            calendar.clearSelection();

            calendar.afterRender(result.CallBackData, true);
            
            calendar.loadingStop();
            
        };

        
        this.$ = function(subid) {
            return document.getElementById(id + "_" + subid);
        };
        
        this.durationHours = function() {
            return 24;
        };
        
        this.businessHoursSpan = function() {
                if (this.businessBeginsHour > this.businessEndsHour) {
                    return 24 - this.businessBeginsHour + this.businessEndsHour;
                }
                else {
                    return this.businessEndsHour - this.businessBeginsHour;
                }
        };
        
        this.rowCount = function() {
            return 48;
        };

        this.eventClickCallBack = function(e, data) { 
            this.callBack2('EventClick', data, e);
        };
        
        this.eventClickDispatch = function (e) {   
            var thisDiv = this;
            
            var e = thisDiv.event;
            
            switch (calendar.eventClickHandling) {
                case 'CallBack':
                    calendar.eventClickCallBack(e);
                    break;
                case 'JavaScript':
                    calendar.onEventClick(e);
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
        
        this.eventResizeDispatch = function (e, shadowHeight, shadowTop, border ) {
            var _startOffset = 1;
            
            var newStart = new Date();
            var newEnd = new Date();
            
            var start = e.start();
            var end = e.end();
            var day = new Date();
            
            var colStart = calendar.columns[e.div.data.DayIndex].Start;
        
            day = end.getDatePart();
            var step = Math.floor((shadowTop + shadowHeight - _startOffset) / calendar.cellHeight);
            var minutes = step * 30;
            var ts = minutes * 60 * 1000;
            
            newStart = start;
            newEnd = colStart.addTime(ts);
            
            switch (calendar.eventResizeHandling) {
                case 'CallBack':
                    calendar.eventResizeCallBack(e, newStart, newEnd);
                    break;
                case 'JavaScript':
                    calendar.onEventResize(e, newStart, newEnd);
                    break;
            }
        };
        
        this.eventMoveCallBack = function(e, newStart, newEnd, newResource, data) { 
            if (!newStart)
                throw 'newStart is null';
            if (!newEnd)
                throw 'newEnd is null';

            var params = {};
            params.e = e;
            params.newStart = newStart;
            params.newEnd = newEnd;
            params.newResource = newResource;
            
            this.callBack2('EventMove', data, params);
        };
        
        this.eventMoveDispatch = function (e, newColumnIndex, shadowTop, ev) {
            var _startOffset = 1;
            var step = Math.floor((shadowTop - _startOffset) / calendar.cellHeight);
            
            var boxStart = step * 30 * 60 * 1000;
            var start = e.start();
            var end = e.end();
            var day = new Date();
            
            if (start.isDayPilotDate) {
                start = start.d;
            }
            day.setTime(Date.UTC(start.getUTCFullYear(), start.getUTCMonth(), start.getUTCDate()));
            
            var startOffset = start.getTime() - (day.getTime() + start.getUTCHours() * 3600 *1000 + Math.floor(start.getUTCMinutes()/30)*30*60*1000 );
            var length = end.getTime() - start.getTime();

            var newColumn = this.columns[newColumnIndex];
            
            var date = newColumn.Start.getTime();
            var newStartUTC = new Date();
            newStartUTC.setTime(date + boxStart + startOffset);
            
            var newStart = new DayPilot.Date(newStartUTC);
            
            var newEnd = newStart.addTime(length);
            
            switch (calendar.eventMoveHandling) {
                case 'CallBack':
                    calendar.eventMoveCallBack(e, newStart, newEnd, newColumn.Value);
                    break;
                case 'JavaScript':
                    calendar.onEventMove(e, newStart, newEnd, newColumn.Value, false);
                    break;
            }
        
        };    
        
        this.timeRangeSelectedCallBack = function(start, end, resource, data) { 
            
            var range = {};
            range.start = start;
            range.end = end;
            range.resource = resource;
            
            this.callBack2('TimeRangeSelected', data, range);
        };   
        
        this.timeRangeSelectedDispatch = function (start, end, column) {   
            // make sure it's DayPilot.Date
            if (!start.isDayPilotDate) {
                start = new DayPilot.Date(start);
            }
            if (!end.isDayPilotDate) {
                end = new DayPilot.Date(end);
            }
            switch (calendar.timeRangeSelectedHandling) {
                case 'CallBack':
                    calendar.timeRangeSelectedCallBack(start, end, column);
                    break;
                case 'JavaScript':
                    calendar.onTimeRangeSelected(start, end, column);
                    break;
            }        
        };

        this.onCellMousedown = function(ev) {
        
            if (DayPilotCalendar.selecting) {
                return;
            }
                
            if (calendar.timeRangeSelectedHandling === "Disabled") {
                return;
            }
            
            var button = (window.event) ? window.event.button : ev.which;    
            if (button !== 1 && button !== 0) {  // Khtml says first button is 0
                return;
            }
        
            DayPilotCalendar.firstMousePos = DayPilot.mc(ev || window.event);
            DayPilotCalendar.selecting = true;
            if (DayPilotCalendar.selectedCells) {
                calendar.clearSelection();
                DayPilotCalendar.selectedCells = [];
            }
            DayPilotCalendar.column = DayPilotCalendar.getColumn(this);
            DayPilotCalendar.selectedCells.push(this);
            DayPilotCalendar.firstSelected = this;
            
            DayPilotCalendar.topSelectedCell = this;
            DayPilotCalendar.bottomSelectedCell = this;

            calendar.activateSelection();
        };
        
        this.activateSelection = function() {
            var selection = this.getSelection();
            
            // color them
            for(var j = 0; j < DayPilotCalendar.selectedCells.length; j++) {
                var cell = DayPilotCalendar.selectedCells[j];
                if (cell && !cell.selected) {
                    var div = document.createElement("div");
                    div.style.height = (calendar.cellHeight - 1) + "px";
                    div.style.backgroundColor = calendar.selectedColor;
                    cell.firstChild.style.display = "none";
                    cell.insertBefore(div, cell.firstChild);
                    
                    cell.selected = div;
                }
            }
        };
        
        this.mousemove = function(ev) {
            
            if (typeof (DayPilotCalendar) === 'undefined') {
                return;
            }
            
            if (!DayPilotCalendar.selecting) {
                return;
            }
                
            var mousePos = DayPilot.mc(ev || window.event);
            
            var thisColumn = DayPilotCalendar.getColumn(this);
            if (thisColumn !== DayPilotCalendar.column) {
                return;
            }
            
            // clean
            calendar.clearSelection();

            // new selected cells
            if (mousePos.y < DayPilotCalendar.firstMousePos.y) {
                DayPilotCalendar.selectedCells = DayPilotCalendar.getCellsBelow(this);
                DayPilotCalendar.topSelectedCell = DayPilotCalendar.selectedCells[0];
                DayPilotCalendar.bottomSelectedCell = DayPilotCalendar.firstSelected;
            }
            else {
                DayPilotCalendar.selectedCells = DayPilotCalendar.getCellsAbove(this);
                DayPilotCalendar.topSelectedCell = DayPilotCalendar.firstSelected;
                DayPilotCalendar.bottomSelectedCell = DayPilotCalendar.selectedCells[0];
            }
            
            calendar.activateSelection();
        };
        
        this.getSelection = function() {
                var start = DayPilotCalendar.topSelectedCell.start;
                var end = DayPilotCalendar.bottomSelectedCell.end;
                var columnId = DayPilotCalendar.topSelectedCell.resource;

                return new DayPilot.Selection(start, end, columnId, calendar); 
        };
        
        this.mouseup = function (ev){
            if (DayPilotCalendar.selecting && DayPilotCalendar.topSelectedCell !== null) {
                DayPilotCalendar.selecting = false;
                
                var sel = calendar.getSelection();
                
                calendar.timeRangeSelectedDispatch(sel.start, sel.end, sel.resource);
                if (calendar.timeRangeSelectedHandling !== "Hold" && calendar.timeRangeSelectedHandling !== "HoldForever") {
                    doNothing();
                }
            }
            else {
                DayPilotCalendar.selecting = false;
            }
        };    

        this.prepareColumns = function() {
            if (!this.columns) {
                this.createDaysViewColumns();
            }
            
            for (var i = 0; i < this.columns.length; i++) {
                this.activateColumn(this.columns[i]);
            }
        };
        
        this.activateColumn = function(column) {
            column.Start = new DayPilot.Date(column.Start);
        
            column.putIntoBlock = function(ep) {
                
                for (var i = 0; i < this.blocks.length; i++) {
                    var block = this.blocks[i];
                    if (block.overlapsWith(ep.Top, ep.Height)) {
                        block.events.push(ep);
                        block.min = Math.min(block.min, ep.Top);
                        block.max = Math.max(block.max, ep.Top + ep.Height);
                        return i;
                    }
                }    
                
                // no suitable block found, create a new one
                var block = [];
                block.lines = [];
                block.events = [];
                
                block.overlapsWith = function(start, width) {
                    var end = start + width - 1;
                    
                    if (!(end < this.min || start > this.max - 1)) {
                        return true;
                    }
                    
                    return false;
                };
                block.putIntoLine = function (ep) {
                    var thisCol = this;
                
                    for (var i = 0; i < this.lines.length; i++) {
                        var line = this.lines[i];
                        if (line.isFree(ep.Top, ep.Height)) {
                            line.push(ep);
                            return i;
                        }
                    }
                    
                    var line = [];
                    line.isFree = function(start, width) {
                        var end = start + width - 1;
                        var max = this.length;
                        
                        for (var i = 0; i < max; i++) {
                            var e = this[i];
                            if (!(end < e.Top || start > e.Top + e.Height - 1)) {
                                return false;
                            }
                        }
                        
                        return true;
                    };
                    
                    line.push(ep);
                    
                    this.lines.push(line);
                    
                    return this.lines.length - 1;
                
                };
               
                block.events.push(ep);
                block.min = ep.Top;
                block.max = ep.Top + ep.Height;
                
                this.blocks.push(block);
                
                return this.blocks.length - 1;
            
            };
            
            column.putIntoLine = function(ep) {
                var thisCol = this;
            
                for (var i = 0; i < this.lines.length; i++) {
                    var line = this.lines[i];
                    if (line.isFree(ep.Top, ep.Height)) {
                        line.push(ep);
                        return i;
                    }
                }
                
                var line = [];
                line.isFree = function(start, width) {
                    var end = start + width - 1;
                    var max = this.length;
                    
                    for (var i = 0; i < max; i++) {
                        var e = this[i];
                        if (!(end < e.Top || start > e.Top + e.Height - 1)) {
                            return false;
                        }
                    }
                    
                    return true;
                };
                
                line.push(ep);
                
                this.lines.push(line);
                
                return this.lines.length - 1;
            };
            
        };
        
        this.createDaysViewColumns = function() {
            this.columns = [];
            
            var start = this.startDate.getDatePart();
            
            for(var i = 0; i < this.days; i++) {
            
                var column = {};
                column.Start = start.addDays(i);
                column.Name = column.Start.toString();
                column.InnerHTML = DayPilot.Date.toLocal(column.Start.d).toLocaleString();
                
                this.columns.push(column);
            }
        };
        
        this.deleteEvents = function() {
        
            if (this.elements.events) {   
                
                for (var i = 0; i < this.elements.events.length; i++) {
                    var div = this.elements.events[i];
                    
                    var object = div.event;
                    if (object) {
                        object.div = null;
                        object.root = null;
                    }
                    
                    div.onclick = null;
                    div.onclickSave = null;
                    div.onmouseover = null;
                    div.onmouseout = null;
                    div.onmousemove = null;
                    div.onmousedown = null;
                    
                    if (div.firstChild && div.firstChild.firstChild && div.firstChild.firstChild.tagName && div.firstChild.firstChild.tagName.toUpperCase() === 'IMG') {
                        var img = div.firstChild.firstChild;
                        img.onmousedown = null;
                        img.onmousemove = null;
                        img.onclick = null;
                        
                    }
                    
                    div.helper = null;
                    div.data = null;
                    div.event = null;
                    
                    DayPilot.de(div);
                }
            }
            this.elements.events = [];
        };
        
        this.drawEvent = function(data) {
            var main = this.nav.main;

            var rounded = true;
            var radius = true;
            var pixels = false;

            var borderColor = this.eventBorderColor;
            
            var div = document.createElement("div");
            div.data = data;
            div.unselectable = 'on';
            div.style.MozUserSelect = 'none';
            div.style.KhtmlUserSelect = 'none';
            div.style.position = 'absolute';
            div.style.fontFamily = this.eventFontFamily;
            div.style.fontSize = this.eventFontSize;
            div.style.color = this.eventFontColor;
            div.style.left = data.Left + '%';
            div.style.top = (data.Top) + 'px';
            div.style.width = data.Width + '%';
            div.style.height = Math.max(data.Height, 2) + 'px';
            if (!rounded) {
                div.style.backgroundColor = borderColor;
            }
            
            div.style.overflow = 'hidden';
            
            div.isFirst = data.PartStart.getTime() === data.Start.getTime();
            div.isLast = data.PartEnd.getTime() === data.End.getTime();

            div.onclick = this.eventClickDispatch;
            
            // inner divs
            var inside = [];
            
            // fix box
            inside.push("<div");

            if (this.showToolTip) {
                inside.push(" title='");
                inside.push(data.ToolTip.replace(/'/g, "&apos;"));
                inside.push("'");
            }

            var height = Math.max(data.Height - 2, 0);

            inside.push(" class='");
            inside.push("'");
            if (pixels) {
                inside.push(" style='margin-top:1px;height:");
                inside.push(height - 2);
            }
            else {
                inside.push(" style='margin-top:0px;height:");
                inside.push(height);
            }
            inside.push("px;background-color:");
            inside.push(this.eventBackColor);
            if (radius) {
                inside.push(";border:1px solid ");
                inside.push(borderColor);
                inside.push(";-moz-border-radius:5px;");
                inside.push(";-webkit-border-radius:5px;");
                inside.push(";border-radius:5px;");
            }
            else {
                inside.push(";border-left:1px solid ");
                inside.push(borderColor);
                inside.push(";border-right:1px solid ");
                inside.push(borderColor);
            }
            inside.push(";");
            inside.push("' unselectable='on'>");

            var headerHeight = this.eventHeaderVisible ? this.eventHeaderHeight : 0;
            
            if (this.eventHeaderVisible) {
            
                var apm = "";
                var hour = data.Start.getHours();
                var am = hour < 12; 
                var minute = data.Start.getMinutes();
                if (this.timeFormat === "Clock12Hours") {
                    hour = hour % 12;
                    if (hour === 0) {
                        hour = 12;
                    }   
                    
                    apm = am ? "am" : "pm";
                }
                if (minute < 10) {
                    minute = "0" + minute;
                }
                
                var text = hour + ":" + minute + apm;

                inside.push("<div unselectable='on' style='overflow:hidden;height:");
                inside.push(this.eventHeaderHeight);
                inside.push("px; background-color:");
                inside.push(borderColor);
                inside.push(";font-size:");
                inside.push(this.eventHeaderFontSize);
                inside.push(";color:");
                inside.push(this.eventHeaderFontColor);
                inside.push("'>");
                inside.push(text);
                inside.push("</div>");
            }
            
            inside.push("<div unselectable='on' style='overflow:hidden;padding-left:2px;height:");
            inside.push(height - headerHeight - 1);
            inside.push("px;'>");
            inside.push(data.InnerHTML);
            inside.push("</div></div>");

            div.innerHTML = inside.join('');

            if (main.rows[0].cells[data.DayIndex]) { // temporary fix for multirow header, but won't hurt later
                var wrapper = main.rows[0].cells[data.DayIndex].firstChild;
                wrapper.appendChild(div);
                
                calendar.makeChildrenUnselectable(div);
                
                var e = new DayPilotCalendar.Event(div, calendar);
            }
            
            calendar.elements.events.push(div);
        };
        
        this.makeChildrenUnselectable = function(el) {
            var c = (el && el.childNodes) ? el.childNodes.length : 0;
            for (var i = 0; i < c; i++) {
                try {
                    var child = el.childNodes[i];
                    if (child.nodeType === 1) {
                        child.unselectable = 'on';
                        this.makeChildrenUnselectable(child);
                    }
                }
                catch (e) {
                }
            }
        };

        this.drawEvents = function() {
            
            var start = new Date();
            
            for (var i = 0; i < this.columns.length; i++) {
                var col = this.columns[i];
                
                for (var m = 0; m < col.blocks.length; m++) {
                    var block = col.blocks[m];
                    for (var j = 0; j < block.lines.length; j++) {
                        var line = block.lines[j];
                        
                        for(var k = 0; k < line.length; k++) {
                            var e = line[k];  
                            
                            e.Width = 100 / block.lines.length;
                            e.Left = e.Width * j;
                            
                            var isLastBlock = (j === block.lines.length - 1);
                            if (!isLastBlock) {
                                e.Width = e.Width * 1.5;
                            }

                            this.drawEvent(e);
                        }
                    }
                }            
            }
            
            var end = new Date();
            var diff = end.getTime() - start.getTime();
        };
        
        this.drawTop = function() {
        
            this.nav.top = document.getElementById(this.id);
            this.nav.top.innerHTML = '';
            
            this.nav.top.style.MozUserSelect = 'none';
            this.nav.top.style.KhtmlUserSelect = 'none';
            this.nav.top.style.position = 'relative';
            this.nav.top.style.width = this.width ? this.width : '100%';
            this.nav.top.style.lineHeight = "1.2";
            this.nav.top.style.textAlign = "left";
            if (this.heightSpec === "Parent100Pct") {
                this.nav.top.style.height = "100%";
            }

            if (this.hideUntilInit) {
                this.nav.top.style.visibility = 'hidden';
            }
            
            this.nav.scroll = document.createElement("div");
            this.nav.scroll.style.height = this.getScrollableHeight() + "px";
            
            if (this.heightSpec === 'BusinessHours') {
                this.nav.scroll.style.overflow = "auto";
            }
            else
            {
                this.nav.scroll.style.overflow = "hidden";
            }

            this.nav.scroll.style.position = "relative";
            this.nav.scroll.style.border = "1px solid " + this.borderColor;
            this.nav.scroll.style.backgroundColor = this.hourNameBackColor;

            var header = this.drawTopHeaderDiv();
            this.nav.top.appendChild(header);
            
            // fixing the column alignment bug
            // solved thanks to http://stackoverflow.com/questions/139000/div-with-overflowauto-and-a-100-wide-table-problem
            this.nav.scroll.style.zoom = 1;

            var wrap = this.drawScrollable();
            this.nav.scrollable = wrap.firstChild;
            this.nav.scroll.appendChild(wrap);
            this.nav.top.appendChild(this.nav.scroll);                

            this.nav.scrollLayer = document.createElement("div");
            this.nav.scrollLayer.style.position = 'absolute';
            this.nav.scrollLayer.style.top = '0px';
            this.nav.scrollLayer.style.left = '0px';
            this.nav.top.appendChild(this.nav.scrollLayer);

            this.nav.loading = document.createElement("div");
            this.nav.loading.style.position = 'absolute';
            this.nav.loading.style.top = '0px';
            this.nav.loading.style.left = (this.hourWidth + 5) + "px";
            this.nav.loading.style.backgroundColor = this.loadingLabelBackColor;
            this.nav.loading.style.fontSize = this.loadingLabelFontSize;
            this.nav.loading.style.fontFamily = this.loadingLabelFontFamily;
            this.nav.loading.style.color = this.loadingLabelFontColor;
            this.nav.loading.style.padding = '2px';
            this.nav.loading.innerHTML = this.loadingLabelText;
            this.nav.loading.style.display = 'none';
        
            this.nav.top.appendChild(this.nav.loading);

        };
        
        // used during full update
        this.drawHourTable = function() {
            // clear old hour table
            if (!this.fasterDispose) DayPilot.pu(this.nav.hourTable);
            this.nav.scrollable.rows[0].cells[0].innerHTML = '';
            this.nav.hourTable = this.createHourTable();
            this.nav.scrollable.rows[0].cells[0].appendChild(this.nav.hourTable);
        };
        
        // used during initial load only
        this.drawScrollable = function() {
            var zoom = document.createElement("div");
            zoom.style.zoom = 1;
            zoom.style.position = 'relative';
        
            var table = document.createElement("table");
            
            table.cellSpacing = "0";
            table.cellPadding = "0";
            table.border = "0";
            table.style.border = "0px none";
            table.style.width = "100%";
            table.style.position = 'absolute';
            
            var r = table.insertRow(-1);
            
            var c;
            c = r.insertCell(-1);
            c.valign = "top";
            c.style.padding = '0px';
            c.style.border = '0px none';
            
            this.nav.hourTable = this.createHourTable();
            c.appendChild(this.nav.hourTable);
            
            c = r.insertCell(-1);
            c.valign = "top";
            c.width = "100%";
            c.style.padding = '0px';
            c.style.border = '0px none';
            
            c.appendChild(this.createEventsAndCells());
            
            zoom.appendChild(table);
            
            this.nav.zoom = zoom;
            
            return zoom;
        };
        
        this.createEventsAndCells = function() {
            var table = document.createElement("table");
            
            table.cellPadding = "0";
            table.cellSpacing = "0";
            table.border = "0";
            table.style.width = "100%";
            table.style.border = "0px none";
            table.style.borderLeft = "1px solid " + this.borderColor;

            this.nav.main = table;
            
            return table;
        
        };
        
        
        this.createHourTable = function() {
            var table = document.createElement("table");
            table.cellSpacing = "0";
            table.cellPadding = "0";
            table.border = "0";
            table.style.border = '0px none';
            table.style.width = this.hourWidth + "px";
            table.oncontextmenu = function() { return false; };
            
            var r = table.insertRow(-1);
            r.style.height = "1px";  // maybe has no effect
            r.style.backgroundColor = "white";
            
            var c = r.insertCell(-1);
            c.style.padding = '0px';
            c.style.border = '0px none';

            var hours = 24;
            for (var i = 0; i < hours; i++) {
                this.createHourRow(table, i);
            }
            
            return table;
        
        };
        
        this.createHourRow = function(table, i) {
            var height = (this.cellHeight * 2);
        
            var r = table.insertRow(-1);
            r.style.height = height + "px";
            
            var c = r.insertCell(-1);
            c.valign = "bottom";
            c.unselectable = "on";
            c.style.backgroundColor = this.hourNameBackColor;
            c.style.cursor = "default";
            c.style.padding = '0px';
            c.style.border = '0px none';
            
            var frame = document.createElement("div");
            frame.style.width = this.hourWidth + "px";
            frame.style.height = (height) + "px";
            frame.style.overflow = 'hidden';
            frame.unselectable = 'on';
            
            var block = document.createElement("div");
            block.style.display = "block";
            block.style.borderBottom = "1px solid " + this.hourNameBorderColor;
            block.style.height = (height - 1) + "px";
            block.style.textAlign = "right";
            block.unselectable = "on";
            
            var text = document.createElement("div");
            text.style.padding = "2px";
            text.style.fontFamily = this.hourFontFamily;
            text.style.fontSize = this.hourFontSize;
            text.style.color = this.hourFontColor;
            text.unselectable = "on";

            var start = this.startDate.addHours(i);
            var hour = start.getHours();
            
            var am = hour < 12; 
            if (this.timeFormat === "Clock12Hours") {
                hour = hour % 12;
                if (hour === 0) {
                    hour = 12;
                }   
            }
            
            text.innerHTML = hour;
            
            var span = document.createElement("span");
            span.style.fontSize = "10px";
            span.style.verticalAlign = "super";
            span.unselectable = "on";
            
            var sup;
            if (this.timeFormat === "Clock12Hours") {
                if (am) {
                    sup = "AM";
                }
                else {
                    sup = "PM";
                }
            }
            else {
                sup = "00";
            }
            
            span.innerHTML = "&nbsp;" + sup;
            
            text.appendChild(span);

            block.appendChild(text);        

            frame.appendChild(block);

            c.appendChild(frame);
        };
        
        this.getScrollableHeight = function() {
            switch (this.heightSpec) {
                case "Full":
                    return (24 * 2 * this.cellHeight);    
                case "BusinessHours":
                    var dHours = this.businessHoursSpan();
                    return dHours * this.cellHeight * 2;
                default:
                    throw "DayPilot.Calendar: Unexpected 'heightSpec' value.";
                    
            }
        };
        
        this.drawTopHeaderDiv = function() {
            var header = document.createElement("div");
            header.style.borderLeft = "1px solid " + this.borderColor;
            header.style.borderRight = "1px solid " + this.borderColor;
            header.style.overflow = "auto";
            
            var table = document.createElement("table");
            table.cellPadding = "0";
            table.cellSpacing = "0";
            table.border = "0";
            table.style.width = "100%";
            table.style.borderCollapse = 'separate';
            table.style.border = "0px none";
            
            var r = table.insertRow(-1);
            
            // corner
            var c = r.insertCell(-1);
            c.style.padding = '0px';
            c.style.border = '0px none';
            
            var corner = this.drawCorner();
            c.appendChild(corner);
            this.nav.corner = corner;
            
            // top header
            c = r.insertCell(-1);
            
            c.style.width = "100%";
            c.style.backgroundColor = this.hourNameBackColor;
            c.valign = "top";
            c.style.position = 'relative';  // ref point
            c.style.padding = '0px';
            c.style.border = '0px none';
            
            this.nav.header = document.createElement("table");
            this.nav.header.cellPadding = "0";
            this.nav.header.cellSpacing = "0";
            this.nav.header.border = "0";
            this.nav.header.width = "100%";
            this.nav.header.style.borderBottom = "0px none #000000";
            this.nav.header.style.borderRight = "0px none #000000";
            this.nav.header.style.borderLeft = "1px solid " + this.borderColor;
            this.nav.header.style.borderTop = "1px solid " + this.borderColor;
            this.nav.header.oncontextmenu = function() { return false; };

            var scrollbar = this.nav.scroll.style.overflow !== 'hidden';
            if (scrollbar) {
                this.nav.header.style.borderRight = "1px solid " + this.borderColor;
            }        
            
            c.appendChild(this.nav.header);

            if (scrollbar) {
                c = r.insertCell(-1);        
                
                c.style.backgroundColor = this.hourNameBackColor;
                c.style.borderTop = "1px solid " + this.borderColor;
                c.style.borderBottom = "0px none";
                c.style.borderLeft = "0px none";
                c.style.borderRight = "0px none";
                c.style.padding = '0px';
                c.style.verticalAlign = 'top';
                c.unselectable = "on";
                c.innerHTML = "&nbsp;";
                
                var inside = document.createElement("div");
                inside.unselectable = "on";
                inside.style.width = "16px";
                inside.style.height = "1px";
                inside.style.lineHeight = "1px";
                inside.style.fontSize = "1px";
                
                c.appendChild(inside);
                
                this.nav.cornerRight = inside;
            }  
            
            header.appendChild(table);
            
            return header;      
        
        };
        
        this.drawCorner = function() {
            var wrap = document.createElement("div");
            wrap.style.position = 'relative';
            wrap.style.backgroundColor = this.hourNameBackColor;
            wrap.style.fontFamily = this.headerFontFamily;
            wrap.style.fontSize = this.headerFontSize;
            wrap.style.color = this.headerFontColor;
            wrap.style.width = this.hourWidth + "px";
            wrap.style.height = this.headerHeight + "px";
            wrap.style.borderTop = "1px solid " + this.borderColor;
            wrap.oncontextmenu = function() { return false; };
            
            var corner = document.createElement("div");
            corner.unselectable = "on";
            
            wrap.appendChild(corner);

            return wrap;
        };
        
        this.disposeMain = function()  {
            var table = this.nav.main;
            table.root = null;
            table.onmouseup = null;
            
            for (var y = 0; y < table.rows.length; y++) {
                var r = table.rows[y];
                for (var x = 0; x < r.cells.length; x++) {
                    var c = r.cells[x];
                    c.root = null;
                    
                    c.onmousedown = null;
                    c.onmousemove = null;
                    c.onmouseout = null;
                    c.onmouseup = null;
                }
            }
            
            if (!this.fasterDispose) DayPilot.pu(table);
        };
        
        // draw time cells
        this.drawMain = function() {
        
            DayPilotCalendar.selectedCells = [];
	        var cols = [];
	        var dates = [];
            
            var table = this.nav.main;
            var step = 30 * 60 * 1000;
            var rowCount = this.rowCount();
            
            var columns = calendar.columns;
	        var create = !this.tableCreated || columns.length !== table.rows[0].cells.length || rowCount !== table.rows.length; // redraw only if number of columns changes
    	    
    	    if (table) {
    	        this.disposeMain();
    	    }
    	    
	        while (table && table.rows && table.rows.length > 0 && create) {
	            if (!this.fasterDispose) DayPilot.pu(table.rows[0]);
	            table.deleteRow(0);
	        }
    	    
	        this.tableCreated = true;

	        var r = (create) ? table.insertRow(-1) : table.rows[0];

            if (create) {	    
	            r.style.backgroundColor = 'white';
	        }

	        var cl = columns.length;

	        for (var j = 0; j < cl; j++) {
	            var c = (create) ? r.insertCell(-1) : r.cells[j];
    	        
	            if (create) {

	                c.style.padding = '0px';
	                c.style.border = '0px none';
	                c.style.height = '1px';
	                c.style.overflow = 'visible';
	                if (!calendar.rtl) {
	                    c.style.textAlign = 'left';
	                }
	                
                    c.style.width = (100.0 / columns.length) + "%";
        	        
	                var div = document.createElement("div");
	                div.style.display = 'block';
	                div.style.marginRight = calendar.columnMarginRight + "px";
	                div.style.position = 'relative';
	                div.style.height = '1px';
	                div.style.fontSize = '1px';
	                div.style.lineHeight = '1.2';
	                div.style.marginTop = '-1px';
        	        
	                c.appendChild(div);
    	            
	            }
	        }

	        for(var i = 0; i < rowCount; i ++) {
		        var r = (create) ? table.insertRow(-1) : table.rows[i + 1];

                if (create) {
		            r.style.MozUserSelect = 'none';
		            r.style.KhtmlUserSelect = 'none';
		        }
    		    
		        for (var j = 0; j < cl; j++) {
		            var col = this.columns[j];
    		        
                    var c = (create) ? r.insertCell(-1) : r.cells[j];
                    
                    // always update
                    c.start = col.Start.addTime(i*step);
                    c.end = c.start.addTime(step);
                    c.resource = col.Value;

                    if (create) {
                        c.root = this;

                        c.style.padding = '0px';
                        c.style.border = '0px none';
                        c.style.verticalAlign = 'top';
                        c.onmousedown = this.onCellMousedown;
                        c.onmousemove = this.mousemove;
                        c.onmouseup = function() { return false; };

                        c.onclick = function() { return false; };
                        
			            if ((!calendar.rtl && j !== cl - 1) || calendar.rtl) {
			                c.style.borderRight = '1px solid ' + calendar.cellBorderColor;  
			            }
    		        
			            c.style.height = calendar.cellHeight + 'px';
			            c.style.overflow = 'hidden';
			            c.unselectable = 'on';
			            
			            var div = document.createElement("div");
			            div.unselectable = 'on';
			            div.style.fontSize = '1px';
			            div.style.height = '0px';
        			    
    			        // hack, no multiplying
                        var endHour = (c.end.getMinutes() + c.end.getSeconds() + c.end.getMilliseconds()) > 0;    			    
        			    			    
			            if (endHour) {
			                if (calendar.hourHalfBorderColor !== '') {
			                    div.style.borderBottom = '1px solid ' + calendar.hourHalfBorderColor; // HourHalfBorderColor
			                }
			            }
			            else {
			                if (calendar.hourBorderColor !== '') {
    			                div.style.borderBottom = '1px solid ' + calendar.hourBorderColor; // HourBorderColor
    			            }
			            }

		                var content = document.createElement("div");
		                content.style.height = (calendar.cellHeight - 1) + "px";
                        content.style.overflow = 'hidden';
                        content.unselectable = 'on';
			            c.appendChild(content);

                        c.appendChild(div);
			            
			        }
    			    
                    c.style.backgroundColor = calendar.cellBackColor;
                    
                    content = c.firstChild;
		        }
	        }
    	    
            table.onmouseup = this.mouseup;
            table.root = this;
            
            var scroll = calendar.nav.scroll;
            
            calendar.nav.scrollable.onmousemove = function (ev) {
            
                ev = ev || window.event; 
            
                var ref = calendar.nav.scrollable;
                calendar.coords = DayPilot.mo3(ref, ev);
                
                var mousePos = DayPilot.mc(ev);
                
                if (DayPilotCalendar.resizing) {
                    if (!DayPilotCalendar.resizingShadow) {
                        DayPilotCalendar.resizingShadow = DayPilotCalendar.createShadow(DayPilotCalendar.resizing, false, calendar.shadow);
                    }
                
                    var _step = DayPilotCalendar.resizing.event.root.cellHeight;
                    var _startOffset = 1;
                    var delta = (mousePos.y - DayPilotCalendar.originalMouse.y);

                    // TODO: clear            	    
                    if (DayPilotCalendar.resizing.dpBorder === 'bottom') {
                        var newHeight = Math.floor( ((DayPilotCalendar.originalHeight + DayPilotCalendar.originalTop + delta) + _step/2) / _step ) * _step - DayPilotCalendar.originalTop + _startOffset;
                        
                        if (newHeight < _step)
                            newHeight = _step;
                            
                        var max = DayPilotCalendar.resizing.event.root.nav.main.clientHeight;
                        if (DayPilotCalendar.originalTop + newHeight > max)
                            newHeight = max - DayPilotCalendar.originalTop;
                            
                        DayPilotCalendar.resizingShadow.style.height = (newHeight - 4) + 'px';
	                }
    	            
                    
                }
                
                else if (DayPilotCalendar.moving) {
                
                    if (!DayPilotCalendar.movingShadow) {
                        // fixes the ie8 bug (incorrect offsetX and offsetY causes flickering during move if there are inline elements in the event
	                    DayPilotCalendar.movingShadow = DayPilotCalendar.createShadow(DayPilotCalendar.moving, !calendar.ie, calendar.shadow);
	                    DayPilotCalendar.movingShadow.style.width = (DayPilotCalendar.movingShadow.parentNode.offsetWidth + 1) + 'px';
                    }
                
                    if (!calendar.coords) {
                        return;
                    }
                    
                    //document.title = "" + calendar.coords.y;

                    var _step = calendar.cellHeight;
                    var _startOffset = 1;
                    
                    var offset = DayPilotCalendar.moveOffsetY;
                    if (!offset) {
                        offset = _step/2; // for external drag
                    }
                    
                    var newTop = Math.floor( ((calendar.coords.y - offset - _startOffset) + _step/2) / _step ) * _step + _startOffset;
                    
                    if (newTop < _startOffset) {
                        newTop = _startOffset;
                    }
                        
                    var main = calendar.nav.main;
                    var max = main.clientHeight;

                    var height = parseInt(DayPilotCalendar.movingShadow.style.height);  // DayPilotCalendar.moving.data.height
                    if (newTop + height > max) {
                        newTop = max - height;
                    }
                    
                    DayPilotCalendar.movingShadow.parentNode.style.display = 'none';
                    DayPilotCalendar.movingShadow.style.top = newTop + 'px';
                    DayPilotCalendar.movingShadow.parentNode.style.display = '';
                    
                    var colWidth = main.clientWidth / main.rows[0].cells.length;
                    var column = Math.floor((calendar.coords.x - 45) / colWidth);
                    
                    if (column < 0) {
                        column = 0;
                    }
                    
                    if (column < main.rows[0].cells.length && column >= 0 && DayPilotCalendar.movingShadow.column !== column) {
                        DayPilotCalendar.movingShadow.column = column;
                        DayPilotCalendar.moveShadow(main.rows[0].cells[column]);
                    }
                    
                }
                
            };

            calendar.nav.scrollable.style.display = '';
        };

        this.disposeHeader = function() {
            var table = this.nav.header;
            if (table && table.rows) {
                for(var y = 0; y < table.rows.length; y++) {
                    var r = table.rows[y];
                    for (var x = 0; x < r.cells.length; x++) {
                        var c = r.cells[x];
                        c.onclick = null;
                        c.onmousemove = null;
                        c.onmouseout = null;
                    }
                }
            }
            if (!this.fasterDispose) DayPilot.pu(table);
        };
        
        this.drawHeaderRow = function(create) {

            // column headers
            var r = (create) ? this.nav.header.insertRow(-1) : this.nav.header.rows[0];
            
            var columns = this.columns;
            
            var len = columns.length;
            for (var i = 0; i < len; i++) {
                var data = columns[i];

                var cell = (create) ? r.insertCell(-1) : r.cells[i];
                cell.data = data;
                
                cell.style.width = (100.0 / columns.length) + "%";

                cell.style.overflow = 'hidden';
                cell.style.padding = '0px';
                cell.style.border = '0px none';
                cell.style.height = (this.headerHeight) + "px";

                var div = (create) ? document.createElement("div") : cell.firstChild;
                
                if (create) {
                    div.unselectable = 'on';
                    div.style.MozUserSelect = 'none';
                    div.style.backgroundColor = data.BackColor;
                    div.style.cursor = 'default';
                    div.style.position = 'relative';
                    div.style.fontFamily = this.headerFontFamily;
                    div.style.fontSize = this.headerFontSize;
                    div.style.color = this.headerFontColor;
                    
                    if (calendar.rtl) {
                        if (i === len -1) { // last one
                            div.style.borderLeft = "1px solid " + data.BackColor;
                        }
                        else {
                            div.style.borderLeft = "1px solid " + this.borderColor;
                        }
                    }
                    else {
                        if (i === len -1) { // last one
                            doNothing();
                        }
                        else {
                            div.style.borderRight = "1px solid " + this.borderColor;
                        }
                    }

                    div.style.height = this.headerHeight + "px";
                    
                    var text = document.createElement("div");
                    text.style.position = 'absolute';
                    text.style.left = '0px';
                    text.style.width = '100%';
                    text.style.padding = "2px";
                    div.style.textAlign = 'center';
                    text.unselectable = 'on';
                   
                    div.appendChild(text);
                    cell.appendChild(div);
                }

                var text = div.firstChild;
                text.innerHTML = data.InnerHTML;
            }
        };
        
        this.widthUnit = function() {
            if (this.width && this.width.indexOf("px") != -1) {
                return "Pixel";
            }
            return "Percentage"; 
        };
        
        this.drawHeader = function() {

            var header = this.nav.header;
            var create = true;

            var columns = this.columns;
            var len = columns.length;
            
            if (this.headerCreated && header && header.rows && header.rows.length > 0) {
                create = header.rows[header.rows.length - 1].cells.length !== len;
            }
            
            while (this.headerCreated && header && header.rows && header.rows.length > 0 && create) {
                if (!this.fasterDispose) DayPilot.pu(header.rows[0]);
                header.deleteRow(0);
            }

            this.headerCreated = true;
            
            if (!create) {
                // corner        
                var corner = calendar.nav.corner;
                corner.style.backgroundColor = calendar.cornerBackColor;
                if (!this.fasterDispose) DayPilot.pu(corner.firstChild);
            }
            
            this.drawHeaderRow(create);
        };
        
        this.loadingStart = function() {
            if (this.loadingLabelVisible) {
                this.nav.loading.innerHTML = this.loadingLabelText;
                this.nav.loading.style.top = (this.headerHeight + 5) + "px";
                this.nav.loading.style.display = '';
            }
        };

        this.commandCallBack = function(command, data) {
            var params = {};
            params.command = command;
            this.callBack2('Command', data, params);
        }; 
        
        this.loadingStop = function(msg) {
            if (this.callbackTimeout) {
                window.clearTimeout(this.callbackTimeout);
            }    
            
            this.nav.loading.style.display = 'none';
        };
        
        this.enableScrolling = function() {
        
            var scrollDiv = this.nav.scroll;
            if (!this.initScrollPos)
                return;
                
            scrollDiv.root = this;
            
            // initial position
            if (scrollDiv.scrollTop === 0) {
                scrollDiv.scrollTop = this.initScrollPos;
            }
        };
        
        this.callbackError = function (result, context) {
            alert("Error!\r\nResult: " + result + "\r\nContext:" + context);
        };

        this.fixScrollHeader = function() {
            var w = DayPilot.sw(this.nav.scroll);
            var d = this.nav.cornerRight;
            if (d && w > 0) { 
                d.style.width = (w - 1) + 'px';
            }
        };
        
        this.registerGlobalHandlers = function() {
            if (!DayPilotCalendar.globalHandlers) {
                DayPilotCalendar.globalHandlers = true;
                DayPilot.re(document, 'mouseup', DayPilotCalendar.gMouseUp);
                DayPilot.re(window, 'unload', DayPilotCalendar.gUnload);
            }
        };


        
        this.loadEvents = function(events) {
            
            if (!events) {
                events = this.events;
            }
            
            var length = events.length;
            var duration = 24 * 60 * 60 * 1000;

            this.cache.pixels = {};

            var loadCache = [];
            
            this.scrollLabels = [];

            this.minStart = 10000;
            this.maxEnd = 0;            
            
            if (!events) {
                return;
            }
            
            for (var i = 0; i < length; i++) {
                var e = events[i];
                e.Start = new DayPilot.Date(e.Start);
                e.End = new DayPilot.Date(e.End);
            }        

            var allStart = this.startDate;
            var allEnd = this.startDate.addDays(this.days);
            
            for(var i = 0; i < this.columns.length; i++) {
            
                var scroll = {};
                scroll.minEnd = 1000000;
                scroll.maxStart = -1;
                this.scrollLabels.push(scroll);
            
                var col = this.columns[i];
                col.events = [];
                col.lines = [];
                col.blocks = [];
            
                var colStart = new DayPilot.Date(col.Start);
                var colStartTicks = colStart.getTime();
                var colEnd = colStart.addTime(duration);
                var colEndTicks = colEnd.getTime();
                
                for (var j = 0; j < length; j++) {
                    if (loadCache[j]) {
                        continue;
                    }
                    
                    var e = events[j];
                    
                    var start = e.Start;
                    var end = e.End;
                    
                    var startTicks = start.getTime();
                    var endTicks = end.getTime();
                    
                    if (endTicks < startTicks) {  // skip invalid events
                        continue;
                    }

                    // belongs here
                    var belongsHere = !(endTicks <= colStartTicks || startTicks >= colEndTicks);
                    
                    if (belongsHere) {
                        var ep = {}; // event part
                        ep.Text = e.Text;
                        ep.Value = e.Value;
                        ep.ToolTip = e.ToolTip ? e.ToolTip : e.Text;
                        ep.Start = start;
                        ep.End = end;
                        ep.DayIndex = i;
                        ep.PartStart = colStartTicks < startTicks ? ep.Start : colStart;
                        ep.PartEnd = colEndTicks > endTicks ? ep.End : colEnd;
                        ep.InnerHTML = e.InnerHTML ? e.InnerHTML : e.Text;
                        
                        var partStartPixels = this.getPixels(ep.PartStart, col.Start);
                        var partEndPixels = this.getPixels(ep.PartEnd, col.Start);

                        var top = partStartPixels.top; 
                        var bottom = partEndPixels.top;
                        
                        // events in the hidden areas
                        if (top === bottom && (partStartPixels.cut || partEndPixels.cut)) {
                            continue;
                        }

                        var boxBottom = partEndPixels.boxBottom;
                    
                        ep.Top = Math.floor(top / this.cellHeight) * this.cellHeight + 1;
                        ep.Height = Math.max(Math.ceil(boxBottom / this.cellHeight) * this.cellHeight - ep.Top, this.cellHeight - 1) + 1;

                        var start = ep.Top;
                        var end = ep.Top + ep.Height;
                        
                        if (start > scroll.maxStart) {
                            scroll.maxStart = start;
                        }
                        if (end < scroll.minEnd) {
                            scroll.minEnd = end; 
                        }
                        
                        if (start < this.minStart) {
                            this.minStart = start;
                        }
                        if (end > this.maxEnd) {
                            this.maxEnd = end;
                        }
                        col.events.push(ep);
                        
                        if (ep.PartStart.getTime() === startTicks && ep.PartEnd.getTime() === endTicks) {
                            loadCache[j] = true;
                        }
                    }
                }
                
            }
            
            
            // sort events inside rows
            for (var i = 0; i < this.columns.length; i++) {
                var col = this.columns[i];
                col.events.sort(this.eventComparer);
                
                // put into lines
                for (var j = 0; j < col.events.length; j++) {
                    var e = col.events[j];
                    col.putIntoBlock(e);
                }
                
                for (var j = 0; j < col.blocks.length; j++) {
                    var block = col.blocks[j];
                    block.events.sort(this.eventComparer);
                    for (var k = 0; k < block.events.length; k++ ) {
                        var e = block.events[k];
                        block.putIntoLine(e);
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
        
        this.getPixels = function(date, start) {
            if (!start) start = this.startDate;
            
            var startTicks = start.getTime();
            var ticks = date.getTime();
            
            var cache = this.cache.pixels[ticks + "_" + startTicks];
            if (cache) { 
                return cache; 
            } 
            
            startTicks = start.getTime();
            
            var boxTicks = 30 * 60 * 1000;
            var topTicks = ticks - startTicks;
            var boxOffsetTicks = topTicks % boxTicks;
            
            var boxStartTicks = topTicks - boxOffsetTicks;
            var boxEndTicks = boxStartTicks + boxTicks;
            if (boxOffsetTicks === 0) {
                boxEndTicks = boxStartTicks;
            }
            
            // it's linear scale so far
            var result = {};
            result.cut = false;
            result.top = this.ticksToPixels(topTicks);
            result.boxTop = this.ticksToPixels(boxStartTicks);
            result.boxBottom = this.ticksToPixels(boxEndTicks);
            
            this.cache.pixels[ticks + "_" + startTicks] = result;
            
            return result;
        };
        
        this.ticksToPixels = function(ticks) { 
            return Math.floor( (this.cellHeight * ticks) / (1000 * 60 * 30) );
        };    
        
        this.prepareVariables = function() {
            this.startDate = new DayPilot.Date(this.startDate);
        };
        
        this.updateHeaderHeight = function() {
            if (this.nav.corner) {
                this.nav.corner.style.height = this.headerHeight + "px";
            }
        };
        
        this.updateHeight = function() {
            var sh = this.getScrollableHeight();
            if (this.nav.scroll && sh > 0) {
                this.nav.scroll.style.height = sh + "px";
            }
        };
        
        this.loadFromServer = function() {
            return (typeof this.events === 'undefined') || !this.events;
        };
        
        this.show = function() {
            if (this.nav.top.style.visibility == 'hidden') {
                this.nav.top.style.visibility = 'visible';
            }
        };
        
        this.initShort = function() {
            this.prepareVariables();
            this.prepareColumns();
            this.drawTop();
            this.drawHeader();
            this.drawMain();
            this.fixScrollHeader();
            this.enableScrolling();
            this.registerGlobalHandlers();
            DayPilotCalendar.register(this);
            this.callBack2('Init');
        };
        
        this.Init = function() {
            var loadFromServer = this.loadFromServer();

            if (loadFromServer) {
                this.initShort();
                return;
            }

            this.prepareVariables();
            this.prepareColumns();
            
            if (this.events) { // are events available?
                this.loadEvents();
            }

            this.drawTop();
            this.drawHeader();
            this.drawMain();
            
            this.show();

            this.fixScrollHeader();
            this.enableScrolling();
            this.registerGlobalHandlers();
            DayPilotCalendar.register(this);

            if (this.events) { // are events available?
                this.updateHeaderHeight();        
                this.drawEvents();
            }

            this.afterRender(null, false);

        };
        
    };
        
    DayPilotCalendar.Cell = function(start, end, column) { 
        this.start = start;
        this.end = end;
        this.column = function() {
            
        };
    };
        
    DayPilotCalendar.Column = function (value, name, date) {
        this.value = value;
        this.name = name;
        this.date = new DayPilot.Date(date);
    };

    DayPilotCalendar.Event = function (object, calendar) {
        object.event = this;
        var ev = this;
        this.div = object;

        this.root = calendar;
        this.calendar = calendar;

        // save the values
        this.value = function() { return object.data.Value; };
        this.id = function() { return object.data.Value; };
        this.text = function() { return object.data.Text; };
        this.start = function() { return object.data.Start; };
        this.end = function() { return object.data.End; };
        this.partStart = function() { return object.data.PartStart; };
        this.partEnd = function() { return object.data.PartEnd; };
        this.innerHTML = function() { var c = object.getElementsByTagName("DIV"); return c[c.length-1].innerHTML;};

        this.toJSON = function(key) {
            var json = {};
            json.value = this.value();
            json.text = this.text();
            json.start = this.start();
            json.end = this.end();
            
            return json;
        };

        object.onmousemove = function (ev) {
            // const
            var resizeMargin = 5;
            var moveMargin = calendar.eventHeaderVisible ? (calendar.eventHeaderHeight) : 10;
            
            if (typeof(DayPilotCalendar) === 'undefined') {
                return;
            }
                
            // position
            var offset = DayPilot.mo3(object, ev);
            if (!offset) {
                return;
            }
            
            if (DayPilotCalendar.resizing || DayPilotCalendar.moving) {
                return;
            }
                
            var isLastPart = this.isLast;
            
            if (offset.y <= moveMargin && calendar.eventMoveHandling !== 'Disabled') {
                this.style.cursor = "move";
            }
            else if (this.offsetHeight - offset.y <= resizeMargin && calendar.eventResizeHandling !== 'Disabled') {
                if (isLastPart) {
                    this.style.cursor = "s-resize";
                    this.dpBorder = 'bottom';
                }
                else {
                    this.style.cursor = 'not-allowed';
                }
            }
            else if (!DayPilotCalendar.resizing && !DayPilotCalendar.moving) {
                if (calendar.eventClickHandling !== 'Disabled') {
                    this.style.cursor = 'pointer';        
                }
                else {
                    this.style.cursor = 'default';        
                }
            }
            
        };
        
        object.onmousedown = function (ev) {
            ev = ev || window.event;
            var button = ev.which || ev.button;
            
            if ((this.style.cursor === 'n-resize' || this.style.cursor === 's-resize') && button === 1) {
                // set
                DayPilotCalendar.resizing = this;
	            DayPilotCalendar.originalMouse = DayPilot.mc(ev);
	            DayPilotCalendar.originalHeight = this.offsetHeight;
	            DayPilotCalendar.originalTop = this.offsetTop;
    	        
            }
            else if (this.style.cursor === 'move' && button === 1) {
                DayPilotCalendar.moving = this;
                DayPilotCalendar.moving.event = this.event;
                var helper = DayPilotCalendar.moving.helper = {};
                helper.oldColumn = calendar.columns[this.data.DayIndex].Value;
	            DayPilotCalendar.originalMouse = DayPilot.mc(ev);
	            DayPilotCalendar.originalTop = this.offsetTop;
    	        
	            var offset = DayPilot.mo3(this, ev);
	            if (offset) {
	                DayPilotCalendar.moveOffsetY = offset.y;
	            }
	            else {
	                DayPilotCalendar.moveOffsetY = 0;
	            }
    	        
	            // cursor
	            document.body.style.cursor = 'move';
            }
            
            return false;
        };
        
    };

    // publish the API 
    DayPilot.Calendar = DayPilotCalendar.Calendar;
	
    // jQuery plugin
    if (typeof jQuery === 'undefined') { return; }
    (function( $ ){
      $.fn.daypilotCalendar = function(options) {
        var first = null;
        var j = this.each(function() {
            if (this.daypilot) { // already initialized
                return;
            };
            
            var daypilot = new DayPilot.Calendar(this.id);
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
    })( jQuery );
    	

})();
