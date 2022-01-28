//diagnosis-create
$(document).ready(function () {
    if (localStorage.key(0) === "create_diagnosis_diabetes") {
        $("#patient_history_tab").trigger("click"),
            $("#ph_diabetes_tab").trigger("click"),
            $("#diagnosis_diabetes").trigger("click");
        localStorage.clear();
        localStorage.removeItem("create_diagnosis_diabetes");
    }
});
//diagnosis-edit
$(document).ready(function () {
    if (localStorage.key(0) === "edit_diagnosis_diabetes") {
        $("#patient_history_tab").trigger("click"),
            $("#ph_diabetes_tab").trigger("click"),
            $("#diagnosis_diabetes").trigger("click");
        localStorage.clear();
        localStorage.removeItem("edit_diagnosis_diabetes");
    }
});
//diagnosis-back
$(document).ready(function () {
    if (localStorage.key(0) === "back_diagnosis_diabetes") {
        $("#patient_history_tab").trigger("click"),
            $("#ph_diabetes_tab").trigger("click"),
            $("#diagnosis_diabetes").trigger("click");
        localStorage.clear();
        localStorage.removeItem("back_diagnosis_diabetes");
    }
});

//add-medication-button
$("#add-diabetes-medication").click(function (e) {
    localStorage.clear();
    localStorage.setItem("add_diabetes_medication", Date.now());
});
//history-medication-button
$("#history-diabetes-medication").click(function (e) {
    localStorage.clear();
    localStorage.setItem("history_diabetes_medication", Date.now());
});
//edit-medication-button
$("#edit-diabetes-medication").click(function (e) {
    localStorage.clear();
    localStorage.setItem("edit_diabetes_medication", Date.now());
});
//medication-create
$(document).ready(function () {
    if (localStorage.key(0) === "create_diabetes_medication") {
        $("#patient_history_tab").trigger("click"),
            $("#ph_diabetes_tab").trigger("click");
        $("#medication_diabetes").trigger("click");
        localStorage.clear();
        localStorage.removeItem("create_diabetes_medication");
    }
});
//medication-edit
$(document).ready(function () {
    if (localStorage.key(0) === "edit_diabetes_medication") {
        $("#patient_history_tab").trigger("click"),
            $("#ph_diabetes_tab").trigger("click");
        $("#medication_diabetes").trigger("click");
        localStorage.clear();
        localStorage.removeItem("edit_diabetes_medication");

    }
});
//medication-back
$(document).ready(function () {
    if (localStorage.key(0) === "back_diabetes_medication") {
        $("#patient_history_tab").trigger("click"),
            $("#ph_diabetes_tab").trigger("click");
        $("#medication_diabetes").trigger("click");
        localStorage.clear();
        localStorage.removeItem("back_diabetes_medication");

    }
});

//vision-create
$(document).ready(function () {
    if (localStorage.key(0) === "create_vision_diabetes") {
        $("#patient_history_tab").trigger("click"),
            $("#ph_diabetes_tab").trigger("click"),
            $("#vision_diabetes").trigger("click");
        localStorage.clear();
        localStorage.removeItem("create_vision_diabetes");
    }
});
//vision-edit
$(document).ready(function () {
    if (localStorage.key(0) === "edit_vision_diabetes") {
        $("#patient_history_tab").trigger("click"),
            $("#ph_diabetes_tab").trigger("click"),
            $("#vision_diabetes").trigger("click");
        localStorage.clear();
        localStorage.removeItem("edit_vision_diabetes");
    }
});
//vision-back
$(document).ready(function () {
    if (localStorage.key(0) === "back_vision_diabetes") {
        $("#patient_history_tab").trigger("click"),
            $("#ph_diabetes_tab").trigger("click"),
            $("#vision_diabetes").trigger("click");
        localStorage.clear();
        localStorage.removeItem("back_vision_diabetes");
    }
});
//podiatry-create
$(document).ready(function () {
    if (localStorage.key(0) === "create_podiatry_diabetes") {
        $("#patient_history_tab").trigger("click"),
            $("#ph_diabetes_tab").trigger("click"),
            $("#podiatry_diabetes").trigger("click");
        localStorage.clear();
        localStorage.removeItem("create_podiatry_diabetes");
    }
});
//podiatry-edit
$(document).ready(function () {
    if (localStorage.key(0) === "edit_podiatry_diabetes") {
        $("#patient_history_tab").trigger("click"),
            $("#ph_diabetes_tab").trigger("click"),
            $("#podiatry_diabetes").trigger("click");
        localStorage.clear();
        localStorage.removeItem("edit_podiatry_diabetes");
    }
});
//podiatry-back
$(document).ready(function () {
    if (localStorage.key(0) === "back_podiatry_diabetes") {
        $("#patient_history_tab").trigger("click"),
            $("#ph_diabetes_tab").trigger("click"),
            $("#podiatry_diabetes").trigger("click");
        localStorage.clear();
        localStorage.removeItem("back_podiatry_diabetes");
    }
});
//autonero-create
$(document).ready(function () {
    if (localStorage.key(0) === "create_autonero_diabetes") {
        $("#patient_history_tab").trigger("click"),
            $("#ph_diabetes_tab").trigger("click"),
            $("#autonero_diabetes").trigger("click");
        localStorage.clear();
        localStorage.removeItem("create_autonero_diabetes");
    }
});
//autonero-edit
$(document).ready(function () {
    if (localStorage.key(0) === "edit_autonero_diabetes") {
        $("#patient_history_tab").trigger("click"),
            $("#ph_diabetes_tab").trigger("click"),
            $("#autonero_diabetes").trigger("click");
        localStorage.clear();
        localStorage.removeItem("edit_autonero_diabetes");
    }
});
//autonero-back
$(document).ready(function () {
    if (localStorage.key(0) === "back_autonero_diabetes") {
        $("#patient_history_tab").trigger("click"),
            $("#ph_diabetes_tab").trigger("click"),
            $("#autonero_diabetes").trigger("click");
        localStorage.clear();
        localStorage.removeItem("back_autonero_diabetes");
    }
});
//hypglycaemia-create
$(document).ready(function () {
    if (localStorage.key(0) === "create_hypo_diabetes") {
        $("#patient_history_tab").trigger("click"),
            $("#ph_diabetes_tab").trigger("click"),
            $("#hypo_diabetes").trigger("click");
        localStorage.clear();
        localStorage.removeItem("create_hypo_diabetes");
    }
});
//hypglycaemia-edit
$(document).ready(function () {
    if (localStorage.key(0) === "edit_hypo_diabetes") {
        $("#patient_history_tab").trigger("click"),
            $("#ph_diabetes_tab").trigger("click"),
            $("#hypo_diabetes").trigger("click");
        localStorage.clear();
        localStorage.removeItem("edit_hypo_diabetes");
    }
});
//hypglycaemia-back
$(document).ready(function () {
    if (localStorage.key(0) === "back_hypo_diabetes") {
        $("#patient_history_tab").trigger("click"),
            $("#ph_diabetes_tab").trigger("click"),
            $("#hypo_diabetes").trigger("click");
        localStorage.clear();
        localStorage.removeItem("back_hypo_diabetes");
    }
});

