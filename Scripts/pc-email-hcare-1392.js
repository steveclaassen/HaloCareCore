$("#doctor-edit").click(function () {
    $("#dr-attachment-overlay").fadeIn(150);
    $("#doctor-email-popup").fadeIn(250);
});

$("#popup-close-btn").click(function () {
    $("#dr-attachment-overlay").fadeOut(150);
    $("#doctor-email-popup").fadeOut(250);
});

$(".popup-close").click(function () {
    $("#dr-attachment-overlay").fadeOut(150);
    $("#doctor-email-popup").fadeOut(250);
});

$(function () {
    $('#dr-email-search-btn').click(function () {
        var doctorname = $('#dr-name-search').val().trim();
        var practicenumber = $('#practice-number-search').val().trim();
        var practicename = $('#practice-name-search').val().trim();

        $("#dr-name-search").val('');
        $("#practice-number-search").val('');
        $("#practice-name-search").val('');
        $('#dr-email-details tr').remove();

        $.ajax({
            url: '/Member/DoctorEmailDetails_01',
            type: "POST",
            dataType: "JSON",
            data: { doctorname: doctorname, practicenumber: practicenumber, practicename: practicename },
            success: function (data) {

                $('#dr-email-details').append('<thead><tr><th style="width: 1%"></th><th style = "width: 12%">Practice#</th><th>Doctor name</th><th>Email address</th><th style = "width: 10%"></th><th style="width: 1%"></th></tr></thead>');

                for (var i = 0; i < data.length; i++) {
                    var html = '';
                    html += '<tr><td></td><td>' + data[i].PracticeNumber + '</td><td>' + data[i].DoctorName + '</td ><td>' + data[i].EmailAddress + '</td ><td><input type="button" class="modal-button-download" name="select-button" id="' + data[i].DoctorID + '" value="Select" onclick="select_one(this.id)"></td ><td></td></tr > ';
                    $('#dr-email-details tr').first().after(html);
                }
            }
        });
    });
})
function select_one(input) {
    $.ajax({
        url: '/Member/GetDoctorEmailInformation',
        type: "POST",
        dataType: "JSON",
        data: { doctorID: input },
        success: function (result) {
            var doctorname = result.DoctorName;
            var practiceno = result.PracticeNumber;
            var emailaddress = result.EmailAddress;

            $('#doctor-information').html('').html(doctorname + ' | Practice # ' + practiceno + '&emsp;');
            $('#doctor-email-to').val(emailaddress);

            email_doctor_clear();
        }
    });

}
function email_doctor_clear() {
    $("#dr-attachment-overlay").fadeOut(150);
    $("#doctor-email-popup").fadeOut(250);
    $("#dr-name-search").val('');
    $("#practice-number-search").val('');
    $("#practice-name-search").val('');
    $('#dr-email-details tr').remove();
}
function close_email_attachments() {
    $("#dr-attachment-overlay").fadeOut(150);
    $("#dr-email-attachment-popup").fadeOut(250);
}


$(document).ready(function () {
    $('#email-2-send').prop('disabled', true);
    $('#emailTemplate2').on("select2:select", function (e) {
        if ($(this).val() > 0) {
            $('#email-message-2').show();
            $('#email-2-send').prop('disabled', false);
            $('#email-2-button').addClass('hidden');
            $('#email-2-send').removeClass('hidden');
            $('#validation-error-2').html('').addClass("hidden");
            $('#template-validation-error-2').html('');
        }
    });
});

$(document).ready(function () {
    $('#email-2-send').click(function () {
        if ($('#doctor-email-to').length == 0) {
            $('#email-validation-error-2').html('<span style="font-size: 8px; color: darkred;vertical-align: middle"><i class="fas fa-exclamation-triangle"></i></span>');
            $('#validation-email-error-2').removeClass("hidden").html('&emsp;<span style="font-size: 8px;vertical-align: middle"> <i class="fas fa-exclamation-triangle"></i></span> <b>Invalid:</b> Email address is required.');
            $('#doctor-email-to').focus();
        }
        else {
            localStorage.setItem("back_email", Date.now());
            $("#email-overlay").fadeOut(150);
            $("#new-email-popup").fadeOut(250);
        }
    });

    $('#email-2-button').click(function () {
        if ($('#emailTemplate2').val() == 0) {
            $('#template-validation-error-2').html('<span style="font-size: 8px; color: darkred;vertical-align: middle"><i class="fas fa-exclamation-triangle"></i></span>');
            $('#validation-error-2').removeClass("hidden").html('&emsp;<span style="font-size: 8px;vertical-align: middle"> <i class="fas fa-exclamation-triangle"></i></span> <b>Invalid:</b> Please select a template from the dropdown menu.');
        }
    });
});

function ValidateEmail2() {
    if ($('#doctor-email-to').val() == "" || $('#doctor-email-to').val() == null) {
        $('#email-validation-error-2').html('<span style="font-size: 8px; color: darkred;vertical-align: middle"><i class="fas fa-exclamation-triangle"></i></span>');
        $('#validation-email-error-2').removeClass("hidden").html('&emsp;<span style="font-size: 8px;vertical-align: middle"> <i class="fas fa-exclamation-triangle"></i></span> <b>Invalid:</b> Email address is required.');
    }
    else {
        $('#email-validation-error-2').html('');
        $('#validation-email-error-2').html('').addClass("hidden");
    }
}

$(document).ready(function () {
    $('#email-2-send').click(function () {
        if ($('#doctor-email-to').length == 0) {
            $('#email-validation-error-2').html('<span style="font-size: 8px; color: darkred;vertical-align: middle"><i class="fas fa-exclamation-triangle"></i></span>');
            $('#validation-email-error-2').removeClass("hidden").html('&emsp;<span style="font-size: 8px;vertical-align: middle"> <i class="fas fa-exclamation-triangle"></i></span> <b>Invalid:</b> Email address is required.');
            $('#doctor-email-to').focus();
        }
        else {
            localStorage.setItem("back_email", Date.now());
            $("#email-overlay").fadeOut(150);
            $("#new-email-popup").fadeOut(250);
        }
    });

    $('#email-2-button').click(function () {
        if ($('#emailTemplate2').val() == null) {
            $('#template-validation-error-2').html('<span style="font-size: 8px; color: darkred;vertical-align: middle"><i class="fas fa-exclamation-triangle"></i></span>');
            $('#validation-error-2').removeClass("hidden").html('&emsp;<span style="font-size: 8px;vertical-align: middle"> <i class="fas fa-exclamation-triangle"></i></span> <b>Invalid:</b> Please select a template from the dropdown menu.');
        }
    });
});

CKEDITOR.replace('emailMessage2', {
    //hcare-1381: toolbar amendment
    toolbar: [
        { name: 'basicstyles', items: ['Bold', 'Italic', 'Underline'] },
        { name: 'clipboard', items: ['Undo', 'Redo'] },
        { name: 'paragraph', items: ['NumberedList', 'BulletedList'] },
        { name: 'hidden', items: ['Image', 'Source', 'NewPage', 'Preview', 'Subscript', 'Superscript', 'TextColor', 'BGColor', '-', 'CopyFormatting', 'RemoveFormat', 'Styles', 'Format', 'Font', 'FontSize', '-', 'Outdent', 'Indent', '-', 'Blockquote', 'JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock', 'Table', 'HorizontalRule', 'SpecialChar'] },
    ],
});
CKEDITOR.config.removePlugins = 'elementspath,contextmenu,liststyle,tabletools,tableselection'; //hcare-1381
CKEDITOR.config.disableNativeSpellChecker = false; //hcare-1381

function EmailTemplate2() {
    //hcare-1384
    var templateID = $('#emailTemplate2').val();
    var membername = $("#e2-member-name").val();

    if (CKEDITOR.instances['emailMessage2'].getData() != '') { CKEDITOR.instances['emailMessage2'].setData(''); }

    $.ajax({
        url: '/Member/EmailTemplateBuild',
        type: "GET",
        dataType: "JSON",
        data: { templateID: templateID },
        success: function (results) {
            if (results.EmailTemplate == null) { results.EmailTemplate = ""; }

            CKEDITOR.instances['emailMessage2'].insertHtml(results.EmailTemplate);
            CKEDITOR.instances['emailMessage2'].setData(CKEDITOR.instances['emailMessage2'].getData().replace("[MemberName]", membername));
        }
    });
}

$(".email-attachment-button-one-2").click(function () {
    $("#dr-attachment-overlay").fadeIn(150);
    $("#dr-email-attachment-popup").fadeIn(250);
});
$(".email-attachment-button-two-2").click(function () {
    $("#dr-attachment-overlay").fadeIn(150);
    $("#dr-email-attachment-popup").fadeIn(250);
});
$("#popup-close-btn-2").click(function () {
    $("#dr-attachment-overlay").fadeOut(150);
    $("#dr-email-attachment-popup").fadeOut(250);
});
$("#popup-close-2").click(function () {
    $("#dr-attachment-overlay").fadeOut(150);
    $("#dr-email-attachment-popup").fadeOut(250);
});

$(document).on('change', 'input[name="attachments-2"]', function () {
    var templateid = $(this).val();
    var dependantid = $('#e2-dependantid').val();
    var programid = $('#e2-programid').val();
    if (this.checked) {
        $('.ea-placeholder-2').hide();
        $('#check-2-' + templateid).removeClass('far fa-circle').addClass('fas fa-check-circle').css({ "color": "rgb(172 209 175 / 0.80)" });
        $('.email-attachment-button-one-2').show();
        $('.email-attachment-button-two-2').hide();

        $.ajax({
            url: '/Member/EmailAttachmentHistoryInsert',
            type: "POST",
            dataType: "JSON",
            data: { templateid: templateid, dependantid: dependantid, programid: programid, status: true },
            success: function (result) {
                $.ajax({
                    url: '/Member/EmailAttachmentHistoryDetails',
                    type: "POST",
                    dataType: "JSON",
                    data: { templateid: templateid },
                    success: function (result) {
                        var data = {};
                        data.d = [
                            { FileName: result.FileName, AttachmentName: result.AttachmentName, Extension: result.AttachmentExtension }];
                        var html = '';
                        for (var i = 0; i < data.d.length; i++)
                            if (data.d[i].Extension == '.docx') {
                                html += '<tr id="' + result.Id + '"><td id="word-icon" style="width: 5%"></td> <td style="width: 85%">' + data.d[i].AttachmentName + '</td > <td style="width: 10%"><input type="checkbox" name="remove-btn-2" id="remove-word-2" value="' + result.Id + '"><label for="remove-word-2" onclick="remove2()" style="margin-bottom: 0px"><i class="fas fa-times-circle delete-attachment"></i ></lable></td ></tr > ';
                            }
                            else if (data.d[i].Extension == '.xlsx') {
                                html += '<tr id="' + result.Id + '"><td id="excel-icon" style="width: 5%"></td> <td style="width: 85%">' + data.d[i].AttachmentName + '<td style="width: 10%"><input type="checkbox" name="remove-btn-2" id="remove-excel-2" value="' + result.Id + '"><label for="remove-excel-2" onclick="remove2()" style="margin-bottom: 0px"><i class="fas fa-times-circle delete-attachment"></i ></lable></td ></tr > ';
                            }
                            else if (data.d[i].Extension == '.pdf') {
                                html += '<tr id="' + result.Id + '"><td id="pdf-icon" style="width: 5%"></td> <td style="width: 85%">' + data.d[i].AttachmentName + '<td style="width: 10%"><input type="checkbox" name="remove-btn-2" id="remove-pdf-2" value="' + result.Id + '"><label for="remove-pdf-2" onclick="remove2()" style="margin-bottom: 0px"><i class="fas fa-times-circle delete-attachment"></i ></lable></td ></tr > ';
                            }
                            else if (data.d[i].Extension == '.jpg') {
                                html += '<tr id="' + result.Id + '"><td id="jpg-icon" style="width: 5%"></td> <td style="width: 85%">' + data.d[i].AttachmentName + '<td style="width: 10%"><input type="checkbox" name="remove-btn-2" id="remove-jpg-2" value="' + result.Id + '"><label for="remove-jpg-2" onclick="remove2()" style="margin-bottom: 0px"><i class="fas fa-times-circle delete-attachment"></i ></lable></td ></tr > ';
                            }
                            else if (data.d[i].Extension == '.txt') {
                                html += '<tr id="' + result.Id + '"><td id="txt-icon" style="width: 5%"></td> <td style="width: 85%">' + data.d[i].AttachmentName + '<td style="width: 10%"><input type="checkbox" name="remove-btn-2" id="remove-txt-2" value="' + result.Id + '"><label for="remove-txt-2" onclick="remove2()" style="margin-bottom: 0px"><i class="fas fa-times-circle delete-attachment"></i ></lable></td ></tr > ';
                            }

                        $('#attachment-table-2 tr').first().after(html);
                    }
                });
            }
        });
    }
    else {
        $('#check-2-' + templateid).addClass('far fa-circle').removeClass('fas fa-check-circle').css({ "color": "rgb(204 204 204 / 0.80)" });
        $.ajax({
            url: '/Member/EmailAttachmentHistoryInsert',
            type: "POST",
            dataType: "JSON",
            data: { templateid: templateid, dependantid: dependantid, programid: programid, status: false },
            success: function (result) {
                $.ajax({
                    url: '/Member/EmailAttachmentHistoryDetails',
                    type: "POST",
                    dataType: "JSON",
                    data: { templateid: templateid },
                    success: function (result) {
                        $('#attachment-table-2 tr#' + result.Id).remove();
                    }
                });
            }
        });
    }
});

function remove2() {
    $(document).on('change', 'input[name="remove-btn-2"]', function () {
        var templateid = $(this).val();
        var dependantid = $('#e2-dependantid').val();
        var programid = $('#e2-programid').val();
        if (this.checked) {
            $.ajax({
                url: '/Member/EmailAttachmentHistoryInsert',
                type: "POST",
                dataType: "JSON",
                data: { templateid: templateid, dependantid: dependantid, programid: programid, status: false },
                success: function (result) {
                    $.ajax({
                        url: '/Member/EmailAttachmentHistoryDetails',
                        type: "POST",
                        dataType: "JSON",
                        data: { templateid: templateid },
                        success: function (result) {
                            $('#attachment-table-2 tr#' + result.Id).hide();
                            $('#attachment-2-' + templateid).prop('checked', false);
                            $('#check-2-' + templateid).addClass('far fa-circle').removeClass('fas fa-check-circle').css({ "color": "rgb(204 204 204 / 0.80)" });
                        }
                    });
                }
            });
        }
    });
}

$(document).on('change', 'input[name="remove-btn-2"]', function () {
    if ($('input[name="remove-btn-2"]:checked').length == $('input[name="remove-btn-2"]').length) {
        $('.email-attachment-button-one').fadeOut(15);
        $('.email-attachment-button-two').fadeIn(25);
        $('.ea-placeholder').fadeIn(25);
    }
});


$(function () {
    $('#doctor-email-to').on('change mouseup mousedown mouseout keydown keyup', function () {
        if ($('#doctor-email-to').val() == "" || $('#doctor-email-to').val() == null) {
            $('#email-validation-error-2').html('<span style="font-size: 8px; color: darkred;vertical-align: middle"><i class="fas fa-exclamation-triangle"></i></span>');
            $('#validation-email-error-2').removeClass("hidden").html('&emsp;<span style="font-size: 8px;vertical-align: middle"> <i class="fas fa-exclamation-triangle"></i></span> <b>Invalid:</b> Email address is required.');
            $('#email-2-send').prop('disabled', true);
        }
        else {
            $('#email-validation-error-2').html('');
            $('#validation-email-error-2').html('').addClass("hidden");
            $('#email-2-send').prop('disabled', false);
        }
    });
});

//disable-submit-button
$('#dr-email-assignment').submit(function () {
    $('#email-2-send').prop('disabled', true);
});