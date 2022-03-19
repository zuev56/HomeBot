$(document).ready(function () {

    //$("#RightPanel").hide();
    LoadUsers(null);

});

function LoadUsers(filterText) {
    $.ajax({
        type: "GET",
        url: "/app/Vk/AjaxGetUsers",
        data: {
            filterText: filterText
        },
        success: function (result) {
            $('#UserList').html(result);
            $("#UserId").val("");
        },
        error: function (error) {
            alert("Ошибка подгрузки списка пользователей")
        }
    });
}

function LoadUsersWithActivity() {
    $.ajax({
        type: "GET",
        url: "/app/Vk/AjaxGetUsersWithActivity",
        data: {
            filterText: $("#usersFilter").val(),
            fromDate: $('#FromDate').val(),
            toDate: $('#ToDate').val()
        },
        success: function (result) {
            $('#UserList').html(result);
        },
        error: function (error) {
            alert("Ошибка подгрузки списка пользователей")
        }
    });
}

function SelectUser(id) {
    $("#UserId").val(id);
    GetPeriodUserActivity();
}

function GetPeriodUserActivity() {
    if ($("#UserId").val() == "")
        return;

    $.ajax({
        type: "GET",
        url: "/app/Vk/AjaxGetUserActivity",
        data: {
            userId: $("#UserId").val(),
            fromDate: $('#FromDate').val(),
            toDate: $('#ToDate').val()
        },
        success: function (result) {
            $('#UserInfo').html(result);
        },
        error: function (error) {
            alert("Ошибка вывода активности пользователя");
        }
    });
}

function GetDetailedUserActivity() {
    if ($("#UserId").val() == "")
        return;

    $.ajax({
        type: "GET",
        url: "/app/Vk/AjaxGetDetailedUserActivity",
        data: { userId: $("#UserId").val() },
        success: function (result) {
            $("#UserInfo").html(result);
        },
        error: function (error) {
            alert("Ошибка выполнения детального анализа");
        }
    });
}

function OnTimeIntervalChanged() {
    if ($("#UserId").val() != "")
        GetPeriodUserActivity();
}

function ShiftDates(daysCount) {
    let fromDate = new Date($('#FromDate').val()).getTime() + (+daysCount * 24 * 60 * 60 * 1000);
    let toDate = new Date($('#ToDate').val()).getTime() + (+daysCount * 24 * 60 * 60 * 1000);

    fromDate = new Date(fromDate);
    toDate = new Date(toDate);

    $('#FromDate').val(fromDate.getFullYear() + '-'
        + GetCorrectDatePartValue(fromDate.getMonth() + 1) + '-'
        + GetCorrectDatePartValue(fromDate.getDate()) + 'T'
        + GetCorrectDatePartValue(fromDate.getHours()) + ':'
        + GetCorrectDatePartValue(fromDate.getMinutes()));
    $('#ToDate').val(toDate.getFullYear() + '-'
        + GetCorrectDatePartValue(toDate.getMonth() + 1) + '-'
        + GetCorrectDatePartValue(toDate.getDate()) + 'T'
        + GetCorrectDatePartValue(toDate.getHours()) + ':'
        + GetCorrectDatePartValue(toDate.getMinutes()));

    if ($("#UserId").val() != "") {
        GetPeriodUserActivity();
    }
    else {
        LoadUsersWithActivity();
    }
}

function GetCorrectDatePartValue(srcValue) {
    return srcValue < 10 ? `0${srcValue}` : srcValue;
}

function GetUserActivity(id, fromDate, toDate) {

}