var checkInTime, checkOutTime, breakStartTime, breakEndTime, weeklyDayOff, publicDayOff;


function timePattern() {
    return /^((\d\d\d\d)\/([0]{0,1}[1-9]|1[012])\/([1-9]|([012][0-9])|(3[01])))\s(([0-1]?[0-9]|2?[0-3]):([0-5]\d))$/g
};

function jSonToStdFormat(JsonStr) {
    var dateSection = JsonStr.split("T")[0];
    var parts = dateSection.split("-");
    return parts[0] + "/" + parts[1] + "/" + parts[2] + " 00:00";
};
function isDayOff(timeString) {
    var wd = getWeekDay(timeString);
    var dd = timeString.split(" ")[0];
    return (weekEnd().includes(wd) || publicHoliday().includes(dd));
};
function createDate(timeString) {
    //yyyy/MM/dd
    var timeParts = timeString.split(" ");
    var dateParts = timeParts[0].split("/");
    return new Date(Date.UTC(Number(dateParts[0]), Number(dateParts[1]) - 1, Number(dateParts[2]), 0, 0, 0, 0));
};
function setMinuteFromClock(timeString) {
    // hh:mm
    var timeParts = timeString.split(" ");
    var clockParts = timeParts[1].split(":");
    return parseInt(clockParts[0]) * 60 + parseInt(clockParts[1]);
};
function workHourStart() {
    return checkInTime;
};
function lunchBreakStart() {
    return breakStartTime;
};

function lunchBreakEnd() {
    return breakEndTime;
};

function workHourFinish() {
    return checkOutTime;
};
function weekEnd(){
    return weeklyDayOff;
};

function publicHoliday() {
    return publicDayOff;
};
function getWeekDay(timeString) {
    var thisDate = createDate(timeString);
    var weekDay = thisDate.getDay();
    switch (weekDay) {
        case 0: return "Sunday"; break;
        case 1: return "Monday"; break;
        case 2: return "Tuesday"; break;
        case 3: return "Wednesday"; break;
        case 4: return "Thursday"; break;
        case 5: return "Friday"; break;
        case 6: return "Saturday"; break;
    };
};
function isTimeInCorrectFormat(timeString) {
    //regex pattern: yyyy/MM/dd hh:mm
    return timePattern().test(timeString);
    //match 1: date, match 7: clock time
};
//
var currentStartTime;
var currentEndTime;
var currentTimeTextBoxId;
function timeStringToMinute(timeString) {
    if (isTimeInCorrectFormat(timeString) == false) return undefined;
    var dateAbsValue = Math.round((createDate(timeString) - createDate("1970/01/01 00:00")) / (1000 * 60));
    return dateAbsValue + setMinuteFromClock(timeString);
};


function pastSunday(timeString) {
    var thisDate = createDate(timeString);
    thisDate.setDate(thisDate.getDate() - thisDate.getDay());
    var thisDateJson = thisDate.toJSON();
    return jSonToStdFormat(thisDateJson);
};

function comingSunday(timeString) {
    var thisDate = createDate(timeString);
    if(thisDate.getDay()!=0) thisDate.setDate(thisDate.getDate() + 7 - thisDate.getDay());
    var thisDateJson = thisDate.toJSON();
    return jSonToStdFormat(thisDateJson);
};

$("#mainContent").on("change", "#from-time, #to-time", function () {
    var startTime = $("#from-time").val();
    var endTime = $("#to-time").val();
    if (startTime == null || startTime == undefined || startTime == '') {
        $('#requiredHrs').val('');
        $('#hourRequired-container').addClass('disp-none');
        return;
    };
    if (endTime == null || endTime == undefined || endTime == '') {
        $('#requiredHrs').val('');
        $('#hourRequired-container').addClass('disp-none');
        return;
    };
    if (startTime != currentStartTime || endTime != currentEndTime) {
        currentStartTime = startTime;
        currentEndTime = endTime;
        var leaveDura = Math.ceil(leaveInMin(startTime, endTime)/60);
        $('#requiredHrs').val(leaveDura.toString());
        if (leaveDura == 0) $('#hourRequired-container').addClass('disp-none');
        if (leaveDura != 0) $('#hourRequired-container').removeClass('disp-none');
    };
});

function normalWorkingMinute() {
    var x = "1970/01/01 ";
    var a = setMinuteFromClock(x + workHourFinish());
    var b = setMinuteFromClock(x + lunchBreakEnd());
    var c = setMinuteFromClock(x + lunchBreakStart());
    var d = setMinuteFromClock(x + workHourStart());
    return a - b + c - d;
};

function inclPubHol(startTimeString, endTimeString) {
    var newPubHol = [];
    var pubHol = publicHoliday();
    var sM = timeStringToMinute(startTimeString);
    var eM = timeStringToMinute(endTimeString);
    pubHol.forEach(function (item, index) {
        var wD = getWeekDay(item + " 00:00");
        var cM = timeStringToMinute(item + " 00:00");
        if (!weekEnd().includes(wD) && cM >= sM && cM <= eM) {
            newPubHol.push(item);
        }
    });
    return newPubHol;
};
function workMinMiddle(startTimeString, endTimeString) {
    var t1 = comingSunday(startTimeString);
    var t2 = pastSunday(endTimeString);
    var diff = timeStringToMinute(t2) - timeStringToMinute(t1);
    if (diff < 0) {
        return undefined;
    }
    else {
        return diff / (60 * 24 * 7) * (7 - weekEnd().length) * normalWorkingMinute()
            - inclPubHol(t1, t2).length * normalWorkingMinute();
    };
};

function inclWkEnd(startTimeString, endTimeString) {
    var a1 = startTimeString.split(" ")[0] + " 00:00";
    var t2 = endTimeString;
    var d2 = timeStringToMinute(t2);
    var i = 0;
    var wD;
    var dd;
    while (timeStringToMinute(a1) <= d2) {
        var wD = getWeekDay(a1);
        if (weekEnd().includes(wD)) i += 1;
        dd = createDate(a1);
        dd.setDate(dd.getDate() + 1);
        a1 = jSonToStdFormat(dd.toJSON());
    };
    return i;
};
function workMinStart(startTimeString, endTimeString) {
    var a1 = startTimeString;
    var wd = createDate(a1);
    wd.setDate(wd.getDate() + 1);
    var t1 = jSonToStdFormat(wd.toJSON());
    var t2 = comingSunday(startTimeString);
    var diff = timeStringToMinute(t2) - timeStringToMinute(t1);
    if (diff < 0) {
        return 0;
    }
    else {
        return (diff/(60*24)+1 - inclWkEnd(t1, t2)
            - inclPubHol(t1, t2).length) * normalWorkingMinute();
    };
};

function workMinEnd(startTimeString, endTimeString) {
    var a1 = pastSunday(endTimeString);
    var wd = createDate(a1);
    wd.setDate(wd.getDate() + 1);
    var t1 = jSonToStdFormat(wd.toJSON());
    var a2 = endTimeString;
    var wd2 = createDate(a2);
    var t2 = jSonToStdFormat(wd2.toJSON());
    var diff = timeStringToMinute(t2) - timeStringToMinute(t1);
    if (diff < 0) {
        return 0;
    }
    else {
        return (diff / (60 * 24) - inclWkEnd(t1, t2)
            - inclPubHol(t1, t2).length) * normalWorkingMinute();
    };
};
function workMinFirst(startTimeString, endTimeString) {
    if (isDayOff(startTimeString)) return 0;
    var t = startTimeString.split(" ")[1];
    var x = "1970/01/01 "
    var leaveS = setMinuteFromClock(x + t);
    var minS = setMinuteFromClock(x + workHourStart());
    var minE = setMinuteFromClock(x + workHourFinish());
    var minLS = setMinuteFromClock(x + lunchBreakStart());
    var minLE = setMinuteFromClock(x + lunchBreakEnd());
    switch (true) {
        case (leaveS <= minS): return normalWorkingMinute();
        case (leaveS <= minLS): return minE - minLE + minLS - leaveS;
        case (leaveS <= minLE): return minE - minLE;
        case (leaveS <= minE): return minE - leaveS;
        case (leaveS > minE): return 0;
    };
};

function workMinLast(startTimeString, endTimeString) {
    if (isDayOff(endTimeString)) return 0;
    var t = endTimeString.split(" ")[1];
    var x = "1970/01/01 "
    var leaveE = setMinuteFromClock(x + t);
    var minS = setMinuteFromClock(x + workHourStart());
    var minE = setMinuteFromClock(x + workHourFinish());
    var minLS = setMinuteFromClock(x + lunchBreakStart());
    var minLE = setMinuteFromClock(x + lunchBreakEnd());
    switch (true) {
        case (leaveE <= minS): return 0;
        case (leaveE <= minLS): return leaveE - minS;
        case (leaveE <= minLE): return minLS - minS;
        case (leaveE <= minE): return leaveE - minLE + minLS - minS;
        case (leaveE > minE): return normalWorkingMinute();
    };
};

function workMinSameWeek(startTimeString, endTimeString){
    var t1 = startTimeString;
    var t2 = endTimeString;
    var a, b, c, d;
    var e, f;
    var g;
    var i = 0;
    a = workMinFirst(t1, t2);
    b = workMinLast(t1, t2);

    c = createDate(t1);
    c.setDate(c.getDate() + 1);
    e = jSonToStdFormat(c.toJSON());

    d = createDate(t2);
    d.setDate(d.getDate() - 1);
    f = jSonToStdFormat(d.toJSON());

    while (timeStringToMinute(e) <= timeStringToMinute(f)) {
        if (!isDayOff(e)) i += 1;
        g = createDate(e);
        g.setDate(g.getDate() + 1);
        e = jSonToStdFormat(g.toJSON());
    };
    return i * normalWorkingMinute();
};

function leaveInMin(startTimeString, endTimeString) {
    var a1 = startTimeString;
    var a2 = endTimeString;
    var c2 = a2.split(" ")[1];
    var d, g;
    while (isDayOff(a1)) {
        d = createDate(a1);
        d.setDate(d.getDate() + 1);
        a1 = jSonToStdFormat(d.toJSON());
    }
    t1 = (a1 == startTimeString) ? startTimeString : a1.split(" ")[0] + " " + workHourStart();
    while (isDayOff(a2)) {
        d = createDate(a2);
        d.setDate(d.getDate() - 1);
        a2 = jSonToStdFormat(d.toJSON());
    }
    t2 = (a2 == endTimeString) ? endTimeString : a2.split(" ")[0] + " " + workHourFinish();

    var dd1 = t1.split(" ")[0];
    var cl1 = t1.split(" ")[1];
    var dd2 = t2.split(" ")[0];
    var cl2 = t2.split(" ")[1];

    var diff = timeStringToMinute(t2) - timeStringToMinute(t1);
    if (diff < 0) return 0;

    if (dd1 == dd2) {
        if (isDayOff(t1)) return 0;
        var x = "1970/01/01 ";
        var minS = setMinuteFromClock(x + workHourStart());
        var minE = setMinuteFromClock(x + workHourFinish());
        var minLS = setMinuteFromClock(x + lunchBreakStart());
        var minLE = setMinuteFromClock(x + lunchBreakEnd());
        var leaveS = setMinuteFromClock(x + cl1);
        var leaveE = setMinuteFromClock(x + cl2);
        switch (true) {
            case (leaveS <= minS): leaveS = minS; break;
            case (leaveS >= minE): leaveS = minE; break;
            case (leaveS >= minLS && leaveS <= minLE): leaveS = minLE; break;
        }
        switch (true) {
            case (leaveE <= minS): leaveE = minS; break;
            case (leaveE >= minE): leaveE = minE; break;
            case (leaveE >= minLS && leaveE <= minLE): leaveE = minLS; break;
        }

        switch (true) {
            case (leaveS >= minS && leaveS <= minLS && leaveE >= minS && leaveE <= minLS): return leaveE - leaveS;
            case (leaveS >= minLE && leaveS <= minE && leaveE >= minLE && leaveE <= minE): return leaveE - leaveS;
            case (leaveS >= minS && leaveS <= minLE && leaveE >= minLE && leaveE <= minE): return minLS - leaveS + leaveE - minLE;
        }
    };
    //---------
    var cS = comingSunday(startTimeString);
    var pS = pastSunday(endTimeString);
    var a, b, c, d, e;

    var diff = timeStringToMinute(pS) - timeStringToMinute(cS);
    if (diff <= 0) {
        a = workMinFirst(t1, t2);
        b = workMinLast(t1, t2);
        c = workMinSameWeek(t1, t2);
        return a + b + c
    };

    if (diff > 0) {
        a = workMinFirst(t1, t2);
        b = workMinLast(t1, t2);
        c = workMinMiddle(t1, t2);
        d = workMinStart(t1, t2);
        e = workMinEnd(t1, t2);
        return a + b + c + d + e;
    };

};

$(document).ready(function () {
    var hrs = $('#requiredHrs').val();
    if (hrs == "0" || hrs == "" || hrs == undefined || hrs == null) {
        $('#hourRequired-container').addClass('disp-none');
    }
    else {
        $('#hourRequired-container').removeClass('disp-none');
    };
});