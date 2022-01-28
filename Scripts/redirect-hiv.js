//diagnosis-create
$(document).ready(function () {
    if (localStorage.key(0) === "create_diagnosis_hiv") {
        $("#patient_history_tab").trigger("click"),
            $("#ph_hiv_tab").trigger("click"),
            $("#diagnosis_hiv").trigger("click");
        localStorage.clear();
        localStorage.removeItem("create_diagnosis_hiv");
    }
});
//diagnosis-edit
$(document).ready(function () {
    if (localStorage.key(0) === "edit_diagnosis_hiv") {
        $("#patient_history_tab").trigger("click"),
            $("#ph_hiv_tab").trigger("click"),
            $("#diagnosis_hiv").trigger("click");
        localStorage.clear();
        localStorage.removeItem("edit_diagnosis_hiv");
    }
});
//diagnosis-back
$(document).ready(function () {
    if (localStorage.key(0) === "back_diagnosis_hiv") {
        $("#patient_history_tab").trigger("click"),
            $("#ph_hiv_tab").trigger("click"),
            $("#diagnosis_hiv").trigger("click");
        localStorage.clear();
        localStorage.removeItem("back_diagnosis_hiv");
    }
});
//add-medication-button
$("#add-hiv-medication").click(function (e) {
    localStorage.clear();
    localStorage.setItem("add_hiv_medication", Date.now());
});
//history-medication-button
$("#history-hiv-medication").click(function (e) {
    localStorage.clear();
    localStorage.setItem("history_hiv_medication", Date.now());
});
//edit-medication-button
$("#edit-hiv-medication").click(function (e) {
    localStorage.clear();
    localStorage.setItem("edit_hiv_medication", Date.now());
});
//medication-create
$(document).ready(function () {
    if (localStorage.key(0) === "create_hiv_medication") {
        $("#patient_history_tab").trigger("click"),
            $("#ph_hiv_tab").trigger("click");
        $("#medication_hiv").trigger("click");
        localStorage.clear();
        localStorage.removeItem("create_hiv_medication");
    }
});
//medication-edit
$(document).ready(function () {
    if (localStorage.key(0) === "edit_hiv_medication") {
        $("#patient_history_tab").trigger("click"),
            $("#ph_hiv_tab").trigger("click");
        $("#medication_hiv").trigger("click");
        localStorage.clear();
        localStorage.removeItem("edit_hiv_medication");

    }
});
//medication-back
$(document).ready(function () {
    if (localStorage.key(0) === "back_hiv_medication") {
        $("#patient_history_tab").trigger("click"),
            $("#ph_hiv_tab").trigger("click");
        $("#medication_hiv").trigger("click");
        localStorage.clear();
        localStorage.removeItem("back_hiv_medication");

    }
});
//otherMedication-create
$(document).ready(function () {
    if (localStorage.key(0) === "create_otherMedication_hiv") {
        $("#patient_history_tab").trigger("click"),
            $("#ph_hiv_tab").trigger("click"),
            $("#otherMedication_hiv").trigger("click");
        localStorage.clear();
        localStorage.removeItem("create_otherMedication_hiv");
    }
});
//otherMedication-edit
$(document).ready(function () {
    if (localStorage.key(0) === "edit_otherMedication_hiv") {
        $("#patient_history_tab").trigger("click"),
            $("#ph_hiv_tab").trigger("click"),
            $("#otherMedication_hiv").trigger("click");
        localStorage.clear();
        localStorage.removeItem("edit_otherMedication_hiv");
    }
});
//otherMedication-back
$(document).ready(function () {
    if (localStorage.key(0) === "back_otherMedication_hiv") {
        $("#patient_history_tab").trigger("click"),
            $("#ph_hiv_tab").trigger("click"),
            $("#otherMedication_hiv").trigger("click");
        localStorage.clear();
        localStorage.removeItem("back_otherMedication_hiv");
    }
});
//bloodResults_hiv-create
$(document).ready(function () {
    if (localStorage.key(0) === "create_bloodResults_hiv") {
        $("#patient_history_tab").trigger("click"),
            $("#ph_hiv_tab").trigger("click"),
            $("#otherMedication_hiv").trigger("click");
        localStorage.clear();
        localStorage.removeItem("create_bloodResults_hiv");
    }
});
//bloodResults_hiv-edit
$(document).ready(function () {
    if (localStorage.key(0) === "edit_bloodResults_hiv") {
        $("#patient_history_tab").trigger("click"),
            $("#ph_hiv_tab").trigger("click"),
            $("#bloodResults_hivbloodResults_hiv").trigger("click");
        localStorage.clear();
        localStorage.removeItem("edit_bloodResults_hiv");
    }
});
//bloodResults_hiv-back
$(document).ready(function () {
    if (localStorage.key(0) === "back_bloodResults_hiv") {
        $("#patient_history_tab").trigger("click"),
            $("#ph_hiv_tab").trigger("click"),
            $("#bloodResults_hiv").trigger("click");
        localStorage.clear();
        localStorage.removeItem("back_bloodResults_hiv");
    }
});
//other-hiv-create
$(document).ready(function () {
    if (localStorage.key(0) === "create_hivOther") {
        $("#patient_history_tab").trigger("click"),
            $("#ph_hiv_tab").trigger("click"),
            $("#general_hiv").trigger("click");
        localStorage.clear();
        localStorage.removeItem("create_hivOther");
    }
});
//other-hiv-edit
$(document).ready(function () {
    if (localStorage.key(0) === "edit_hivOther") {
        $("#patient_history_tab").trigger("click"),
            $("#ph_hiv_tab").trigger("click"),
            $("#general_hiv").trigger("click");
        localStorage.clear();
        localStorage.removeItem("edit_hivOther");
    }
});
//other-hiv-back
$(document).ready(function () {
    if (localStorage.key(0) === "back_hivOther") {
        $("#patient_history_tab").trigger("click"),
            $("#ph_hiv_tab").trigger("click"),
            $("#general_hiv").trigger("click");
        localStorage.clear();
        localStorage.removeItem("back_hivOther");
    }
});
