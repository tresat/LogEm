$(function () {
    $.ajaxSetup({
        type: 'POST',
        contentType: 'application/json; charset=utf-8',
        data: {},
        cache: false,
        dataFilter: function (data) {
            var msg;

            if (typeof (JSON) !== 'undefined' &&
        typeof (JSON.parse) === 'function')
                msg = JSON.parse(data);
            else
                msg = eval('(' + data + ')');

            if (msg.hasOwnProperty('d'))
                return msg.d;
            else
                return msg;
        },
        error: function (pResponse) {
            alert('WS error!');
        },
        failure: function (pResponse) {
            alert('WS failure!');
        }
    });
});