//clinical-create
$(document).ready(function () {
    if (localStorage.key(0) === "create_clinicalExam") {
        $("#patient_history_tab").trigger("click"),
            $("#patientInfo").trigger("click");
        localStorage.clear();
        localStorage.removeItem("create_clinicalExam");
    }
});
//clinical-edit
$(document).ready(function () {
    if (localStorage.key(0) === "edit_clinicalExam") {
        $("#patient_history_tab").trigger("click"),
            $("#patientInfo").trigger("click");
        localStorage.clear();
        localStorage.removeItem("edit_clinicalExam");
    }
});
//clinical-back
$(document).ready(function () {
    if (localStorage.key(0) === "back_clinicalExam") {
        $("#patient_history_tab").trigger("click"),
            $("#patientInfo").trigger("click");
        localStorage.clear();
        localStorage.removeItem("back_clinicalExam");
    }
});
//comorbid-create
$(document).ready(function () {
    if (localStorage.key(0) === "create_comorbid") {
        $("#patient_history_tab").trigger("click"),
            $("#comorbid").trigger("click");
        localStorage.clear();
        localStorage.removeItem("create_comorbid");
    }
});
//comorbid-edit
$(document).ready(function () {
    if (localStorage.key(0) === "edit_comorbid") {
        $("#patient_history_tab").trigger("click"),
            $("#comorbid").trigger("click");
        localStorage.clear();
        localStorage.removeItem("edit_comorbid");
    }
});
//comorbid-back
$(document).ready(function () {
    if (localStorage.key(0) === "back_comorbid") {
        $("#patient_history_tab").trigger("click"),
            $("#comorbid").trigger("click");
        localStorage.clear();
        localStorage.removeItem("back_comorbid");
    }
});

//add-medication-button
$("#add-general-medication").click(function (e) {
    localStorage.clear();
    localStorage.setItem("add_medication", Date.now());
});
//history-medication-button
$("#history-general-medication").click(function (e) {
    localStorage.clear();
    localStorage.setItem("history_medication", Date.now());
});
//edit-medication-button
$("#edit-general-medication").click(function (e) {
    localStorage.clear();
    localStorage.setItem("edit_medication", Date.now());
});
//medication-create
$(document).ready(function () {
    if (localStorage.key(0) === "create_medication") {
        $("#patient_history_tab").trigger("click"),
            $("#general_medication").trigger("click");
        localStorage.clear();
        localStorage.removeItem("create_medication");
    }
});
//medication-edit
$(document).ready(function () {
    if (localStorage.key(0) === "edit_medication") {
        $("#patient_history_tab").trigger("click"),
            $("#general_medication").trigger("click");
        localStorage.clear();
        localStorage.removeItem("edit_medication");

    }
});
//medication-back
$(document).ready(function () {
    if (localStorage.key(0) === "back_medication") {
        $("#patient_history_tab").trigger("click"),
            $("#general_medication").trigger("click");
        localStorage.clear();
        localStorage.removeItem("back_medication");

    }
});

//allergy-create
$(document).ready(function () {
    if (localStorage.key(0) === "create_allergy") {
        $("#patient_history_tab").trigger("click"),
            $("#general_allergy").trigger("click");
        localStorage.clear();
        localStorage.removeItem("create_allergy");
    }
});
//allergy-edit
$(document).ready(function () {
    if (localStorage.key(0) === "edit_allergy") {
        $("#patient_history_tab").trigger("click"),
            $("#general_allergy").trigger("click");
        localStorage.clear();
        localStorage.removeItem("edit_allergy");
    }
});
//allergy-back
$(document).ready(function () {
    if (localStorage.key(0) === "back_allergy") {
        $("#patient_history_tab").trigger("click"),
            $("#general_allergy").trigger("click");
        localStorage.clear();
        localStorage.removeItem("back_allergy");
    }
});

//socialHistory-create
$(document).ready(function () {
    if (localStorage.key(0) === "create_socialRecord") {
        $("#patient_history_tab").trigger("click"),
            $("#general_socialHistory").trigger("click");
        localStorage.clear();
        localStorage.removeItem("create_socialRecord");
    }
});
//socialHistory-edit
$(document).ready(function () {
    if (localStorage.key(0) === "edit_socialRecord") {
        $("#patient_history_tab").trigger("click"),
            $("#general_socialHistory").trigger("click");
        localStorage.clear();
        localStorage.removeItem("edit_socialRecord");
    }
});
//socialHistory-back
$(document).ready(function () {
    if (localStorage.key(0) === "back_socialRecord") {
        $("#patient_history_tab").trigger("click"),
            $("#general_socialHistory").trigger("click");
        localStorage.clear();
        localStorage.removeItem("back_socialRecord");
    }
});

//hospitalisations-create
$(document).ready(function () {
    if (localStorage.key(0) === "create_hospitalisation") {
        $("#patient_history_tab").trigger("click"),
            $("#general_hospital").trigger("click");
        localStorage.clear();
        localStorage.removeItem("create_hospitalisation");
    }
});
//hospitalisations-edit
$(document).ready(function () {
    if (localStorage.key(0) === "edit_hospitalisation") {
        $("#patient_history_tab").trigger("click"),
            $("#general_hospital").trigger("click");
        localStorage.clear();
        localStorage.removeItem("edit_hospitalisation");
    }
});
//hospitalisations-back
$(document).ready(function () {
    if (localStorage.key(0) === "back_hospitalisation") {
        $("#patient_history_tab").trigger("click"),
            $("#general_hospital").trigger("click");
        localStorage.clear();
        localStorage.removeItem("back_hospitalisation");
    }
});

//pregnant-create
$(document).ready(function () {
    if (localStorage.key(0) === "create_pregnant") {
        $("#patient_history_tab").trigger("click"),
            $("#general_pregnant").trigger("click");
        localStorage.clear();
        localStorage.removeItem("create_pregnant");
    }
});
//pregnant-edit
$(document).ready(function () {
    if (localStorage.key(0) === "edit_pregnant") {
        $("#patient_history_tab").trigger("click"),
            $("#general_pregnant").trigger("click");
        localStorage.clear();
        localStorage.removeItem("edit_pregnant");
    }
});
//pregnant-back
$(document).ready(function () {
    if (localStorage.key(0) === "back_pregnant") {
        var key = localStorage.key(0);
        $("#patient_history_tab").trigger("click"),
            $("#general_pregnant").trigger("click"),
            localStorage.removeItem("back_pregnant"),
            localStorage.clear();

    }
});

//general-create
$(document).ready(function () {
    if (localStorage.key(0) === "create_clinicalGeneral") {
        $("#patient_history_tab").trigger("click"),
            $("#clinicalGeneral").trigger("click");
        localStorage.clear();
        localStorage.removeItem("create_clinicalGeneral");
    }
});

//general-edit
$(document).ready(function () {
    if (localStorage.key(0) === "edit_clinicalGeneral") {
        $("#patient_history_tab").trigger("click"),
            $("#clinicalGeneral").trigger("click");
        localStorage.clear();
        localStorage.removeItem("edit_clinicalGeneral");
    }
});
//general-back
$(document).ready(function () {
    if (localStorage.key(0) === "back_clinicalGeneral") {
        var key = localStorage.key(0);
        $("#patient_history_tab").trigger("click"),
            $("#clinicalGeneral").trigger("click"),
            localStorage.removeItem("back_clinicalGeneral"),
            localStorage.clear();

    }
});
//newborn-create
$(document).ready(function () {
    if (localStorage.key(0) === "create_newborn") {
        $("#patient_history_tab").trigger("click"),
            $("#general_newborn").trigger("click");
        localStorage.clear();
        localStorage.removeItem("create_newborn");
    }
});
//newborn-edit
$(document).ready(function () {
    if (localStorage.key(0) === "edit_newborn") {
        $("#patient_history_tab").trigger("click"),
            $("#general_newborn").trigger("click");
        localStorage.clear();
        localStorage.removeItem("edit_newborn");
    }
});
//newborn-back
$(document).ready(function () {
    if (localStorage.key(0) === "back_newborn") {
        $("#patient_history_tab").trigger("click"),
            $("#general_newborn").trigger("click"),
            localStorage.removeItem("back_newborn"),
            localStorage.clear();
    }
});


