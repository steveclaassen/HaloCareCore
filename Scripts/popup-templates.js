$(function () {
    var prog_check = $('#program-check').val();
    var prof_check = $('#profile-check').val();
    var program = $('#program-pop').val();
    var profile = $('#profile-pop').val();

    //alert(prog_check);
    //alert(program);
    //alert(prof_check);
    //alert(profile);

    if (((prog_check.toLowerCase() == "true" && program.toLowerCase() == "true") || (prog_check.toLowerCase() == "" && program.toLowerCase() == "true")) &&
        ((prof_check.toLowerCase() == "true" && profile.toLowerCase() == "true") || (prof_check.toLowerCase() == "" && profile.toLowerCase() == "true"))) {
        //alert(1);
        $("#popup-overlay").show();
        $("#popup-warning").fadeIn(250);

        $("#popup-submit").click(function () {
            $("#popup-overlay").fadeToggle(150);
            $("#popup-warning").fadeToggle(250);
            $(".popup-profile").fadeIn(250);
            $("#popup-warning-two").fadeIn(250);
            ProgramPopUpChange();
            ProfilePopChange();
        });

        $(".popup-close").click(function () {
            $("#popup-overlay").fadeToggle(150);
            $("#popup-warning").fadeToggle(250);
            ProgramPopUpChange();
            ProfilePopChange();
        });

        $(function () {
            $('input[type="checkbox"]#profile-popup').prop('checked', true);
            $('#profile-checkbox').removeClass("popup-default");
            $('#profile-checkbox').addClass("popup-checked");
        });

        ProfilePopUpChange();

    }
    else if (((prog_check.toLowerCase() == "false" && program.toLowerCase() == "true") && (prof_check.toLowerCase() == "true" && profile.toLowerCase() == "true"))) {
        //alert(2);
        $("#popup-warning-two").fadeIn(250);
        $(".popup-profile").fadeIn(250);

        $(function () {
            $('input[type="checkbox"]#profile-popup').prop('checked', true);
            $('#profile-checkbox').removeClass("popup-default");
            $('#profile-checkbox').addClass("popup-checked");
        });

        ProfilePopUpChange();

    }
    else if (((prog_check.toLowerCase() == "false" && program.toLowerCase() == "false") && (prof_check.toLowerCase() == "true" && profile.toLowerCase() == "true"))) {
        //alert(3);
        $(".popup-profile").fadeIn(250);

        $(function () {
            $('input[type="checkbox"]#profile-popup').prop('checked', false);
            $('#profile-checkbox').addClass("popup-default");
            $('#profile-checkbox').removeClass("popup-checked");
        });

        ProfilePopUpChange();

    }
    else if (((prog_check.toLowerCase() == "false" && program.toLowerCase() == "true") && (prof_check.toLowerCase() == "false" && profile.toLowerCase() == "true"))) {
        //alert(4);
        $(".popup-profile").fadeIn();

        $(function () {
            $('input[type="checkbox"]#profile-popup').prop('checked', false);
            $('#profile-checkbox').addClass("popup-default");
            $('#profile-checkbox').removeClass("popup-checked");
        });

        ProfilePopUpChange();

    }

    else if ((prog_check.toLowerCase() == "" && program.toLowerCase() == "true")) {
        //alert(5);
        $("#popup-overlay").show();
        $("#popup-warning").fadeIn(250);

        $(".popup-profile").hide();

        $("#popup-submit").click(function () {
            $("#popup-overlay").fadeToggle(150);
            $("#popup-warning").fadeToggle(250);
            ProgramPopUpChange();
        });

        $(".popup-close").click(function () {
            $("#popup-overlay").fadeToggle(150);
            $("#popup-warning").fadeToggle(250);
            ProgramPopUpChange();
        });

        $(function () {
            $('input[type="checkbox"]#profile-popup').prop('checked', false);
            $('#profile-checkbox').addClass("popup-default");
            $('#profile-checkbox').removeClass("popup-checked");
        });

        ProfilePopUpChange();

    }
    else if ((prog_check.toLowerCase() == "true" && program.toLowerCase() == "true")) {
        //alert(6);
        $("#popup-overlay").show();
        $("#popup-warning").fadeIn(250);

        $(".popup-profile").hide();

        $("#popup-submit").click(function () {
            $("#popup-overlay").fadeToggle(150);
            $("#popup-warning").fadeToggle(250);
            ProgramPopUpChange();
        });

        $(".popup-close").click(function () {
            $("#popup-overlay").fadeToggle(150);
            $("#popup-warning").fadeToggle(250);
            ProgramPopUpChange();
        });

        $(function () {
            $('input[type="checkbox"]#profile-popup').prop('checked', false);
            $('#profile-checkbox').addClass("popup-default");
            $('#profile-checkbox').removeClass("popup-checked");
        });

        ProfilePopUpChange();

    }
    else if ((prog_check.toLowerCase() == "false" && program.toLowerCase() == "true")) {
        //alert(7);
        $(".popup-profile").hide();

        $(function () {
            $('input[type="checkbox"]#profile-popup').prop('checked', false);
            $('#profile-checkbox').addClass("popup-default");
            $('#profile-checkbox').removeClass("popup-checked");
        });

        ProfilePopUpChange();
    }

    else if ((prof_check.toLowerCase() == "" && profile.toLowerCase() == "true")) {
        //alert(8);
        $("#popup-warning-two").fadeIn(250);
        $(".popup-profile").fadeIn(250);

        $(function () {
            $('input[type="checkbox"]#profile-popup').prop('checked', true);
            $('#profile-checkbox').removeClass("popup-default");
            $('#profile-checkbox').addClass("popup-checked");
        });

        ProfilePopUpChange();
    }
    else if ((prof_check.toLowerCase() == "true" && profile.toLowerCase() == "true")) {
        //alert(9);
        $("#popup-warning-two").fadeIn(250);
        $(".popup-profile").fadeIn(250);

        $(function () {
            $('input[type="checkbox"]#profile-popup').prop('checked', true);
            $('#profile-checkbox').removeClass("popup-default");
            $('#profile-checkbox').addClass("popup-checked");
        });

        ProfilePopUpChange();
    }
    else if ((prof_check.toLowerCase() == "false" && profile.toLowerCase() == "true")) {
        //alert(10);
        $(".popup-profile").fadeIn(250);

        $(function () {
            $('input[type="checkbox"]#profile-popup').prop('checked', false);
            $('#profile-checkbox').addClass("popup-default");
            $('#profile-checkbox').removeClass("popup-checked");
        });

        ProfilePopUpChange();
    }

})

//profile-popup-only
$(".popup-close-short").click(function () {
    $("#popup-warning-two").fadeOut(250);
    $('#profile-checkbox').removeClass("popup-checked");
    $('#profile-checkbox').addClass("popup-default");
    $('input[type="checkbox"]#profile-popup').prop('checked', false);

    var dependantID = $("#dependantid").val();
    var programID = $("#programid").val();
    var username = $("#user-name").val();

    if (this.checked) {
        $.ajax({
            url: '/Member/ProfilePopRecord',
            type: "POST",
            dataType: "JSON",
            data: { dependantID: dependantID, programID: programID, username: username, check: false },
            success: function (result) {
            }
        });
    }
    else {
        $.ajax({
            url: '/Member/ProfilePopRecord',
            type: "POST",
            dataType: "JSON",
            data: { dependantID: dependantID, programID: programID, username: username, check: false },
            success: function (result) {
            }
        });
    }
});

$('input[type="checkbox"]#profile-popup').change(function (event) {
    if (this.checked) {
        $('#profile-checkbox').removeClass("popup-default");
        $('#profile-checkbox').addClass("popup-checked");
        $("#popup-warning-two").fadeIn(250);
    }
    else {
        $('#profile-checkbox').removeClass("popup-checked");
        $('#profile-checkbox').addClass("popup-default");
        $("#popup-warning-two").fadeOut(250);
    }
});
$('input[type="checkbox"]#profile-popup').change(function (event) {
    var dependantID = $("#dependantid").val();
    var programID = $("#programid").val();
    var username = $("#user-name").val();

    if (this.checked) {
        $.ajax({
            url: '/Member/ProfilePopRecord',
            type: "POST",
            dataType: "JSON",
            data: { dependantID: dependantID, programID: programID, username: username, check: true },
            success: function (result) {
            }
        });
    }
    else {
        $.ajax({
            url: '/Member/ProfilePopRecord',
            type: "POST",
            dataType: "JSON",
            data: { dependantID: dependantID, programID: programID, username: username, check: false },
            success: function (result) {
            }
        });
    }
});

function ProfilePopUpChange() {

    $('input[type="checkbox"]#profile-popup').change(function (event) {
        if (this.checked) {
            $('#profile-checkbox').removeClass("popup-default");
            $('#profile-checkbox').addClass("popup-checked");
            $("#popup-warning-two").fadeIn(250);
        }
        else {
            $('#profile-checkbox').removeClass("popup-checked");
            $('#profile-checkbox').addClass("popup-default");
            $("#popup-warning-two").fadeOut(250);
        }
    });
    $('input[type="checkbox"]#profile-popup').change(function (event) {
        var dependantID = $("#dependantid").val();
        var programID = $("#programid").val();
        var username = $("#user-name").val();

        if (this.checked) {
            $.ajax({
                url: '/Member/ProfilePopRecord',
                type: "POST",
                dataType: "JSON",
                data: { dependantID: dependantID, programID: programID, username: username, check: true },
                success: function (result) {
                }
            });
        }
        else {
            $.ajax({
                url: '/Member/ProfilePopRecord',
                type: "POST",
                dataType: "JSON",
                data: { dependantID: dependantID, programID: programID, username: username, check: false },
                success: function (result) {
                }
            });
        }
    });
}

function ProgramPopUpChange() {
    var dependantID = $("#dependantid").val();
    var programID = $("#programid").val();
    var username = $("#user-name").val();

    $.ajax({
        url: '/Member/ProgramPopRecord',
        type: "POST",
        dataType: "JSON",
        data: { dependantID: dependantID, programID: programID, username: username, check: false },
        success: function (result) {
        }
    });
}
function ProfilePopChange() {
    var dependantID = $("#dependantid").val();
    var programID = $("#programid").val();
    var username = $("#user-name").val();

    $.ajax({
        url: '/Member/ProfilePopRecord',
        type: "POST",
        dataType: "JSON",
        data: { dependantID: dependantID, programID: programID, username: username, check: true },
        success: function (result) {
        }
    });
}