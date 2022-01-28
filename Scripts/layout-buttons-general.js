//left-navigation-slider
$("#menu-toggle").click(function (e) {
    e.preventDefault();
    $("#wrapper").toggleClass("active");
});
//waffle-navigation-script-open
$("#patientMenu-toggle").click(function (e) {
    e.preventDefault();
    $("#patientWrapper").toggleClass("active");
});
//waffle-navigation-script-close
$("#exit-toggle").click(function (e) {
    e.preventDefault();
    $("#patientWrapper").toggleClass("active");
});
//special-note
$("#alert-toggle").click(function (e) {
    e.preventDefault();
    $("#alertWrapper").toggleClass("active");
});
//program-popup
$("#program-toggle").click(function (e) {
    e.preventDefault();
    $("#programWrapper").toggleClass("active");
});
//go-back
$(function () {
    $('.go_back').click(function (e) {
        e.preventDefault();
        history.go(-1);
    });
});
