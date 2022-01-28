//dsm5-create
$(document).ready(function () {
    if (localStorage.key(0) === "create_dsm5") {
        $("#patient_history_tab").trigger("click"),
            $("#ph_mentalhealth_tab").trigger("click"),
            $("#mh-dsm5").trigger("click"),
            localStorage.clear(),
            localStorage.removeItem("create_dsm5");
    }
});
//dsm5-edit
$(document).ready(function () {
    if (localStorage.key(0) === "edit_dsm5") {
        $("#patient_history_tab").trigger("click"),
            $("#ph_mentalhealth_tab").trigger("click"),
            $("#mh-dsm5").trigger("click"),
            localStorage.clear(),
            localStorage.removeItem("edit_dsm5");
    }
});
//dsm5-back
$(document).ready(function () {
    if (localStorage.key(0) === "back_dsm5") {
        $("#patient_history_tab").trigger("click"),
            $("#ph_mentalhealth_tab").trigger("click"),
            $("#mh-dsm5").trigger("click"),
            localStorage.removeItem("back_dsm5"),
            localStorage.clear();
    }
});

//schizophrenia-create
$(document).ready(function () {
    if (localStorage.key(0) === "create_schizophrenia") {
        $("#patient_history_tab").trigger("click"),
            $("#ph_mentalhealth_tab").trigger("click"),
            $("#mh-schizophrenia").trigger("click"),
            localStorage.clear(),
            localStorage.removeItem("create_schizophrenia");
    }
});
//schizophrenia-edit
$(document).ready(function () {
    if (localStorage.key(0) === "edit_schizophrenia") {
        $("#patient_history_tab").trigger("click"),
            $("#ph_mentalhealth_tab").trigger("click"),
            $("#mh-schizophrenia").trigger("click"),
            localStorage.clear(),
            localStorage.removeItem("edit_schizophrenia");
    }
});
//schizophrenia-back
$(document).ready(function () {
    if (localStorage.key(0) === "back_schizophrenia") {
        $("#patient_history_tab").trigger("click"),
            $("#ph_mentalhealth_tab").trigger("click"),
            $("#mh-schizophrenia").trigger("click"),
            localStorage.removeItem("back_schizophrenia"),
            localStorage.clear();
    }
});

//dsm5-create
$(document).ready(function () {
    if (localStorage.key(0) === "create_bipolar") {
        $("#patient_history_tab").trigger("click"),
            $("#ph_mentalhealth_tab").trigger("click"),
            $("#mh-bipolar").trigger("click"),
            localStorage.clear(),
            localStorage.removeItem("create_bipolar");
    }
});
//dsm5-edit
$(document).ready(function () {
    if (localStorage.key(0) === "edit_bipolar") {
        $("#patient_history_tab").trigger("click"),
            $("#ph_mentalhealth_tab").trigger("click"),
            $("#mh-bipolar").trigger("click"),
            localStorage.clear(),
            localStorage.removeItem("edit_bipolar");
    }
});
//dsm5-back
$(document).ready(function () {
    if (localStorage.key(0) === "back_bipolar") {
        $("#patient_history_tab").trigger("click"),
            $("#ph_mentalhealth_tab").trigger("click"),
            $("#mh-bipolar").trigger("click"),
            localStorage.removeItem("back_bipolar"),
            localStorage.clear();
    }
});
//depression-create
$(document).ready(function () {
    if (localStorage.key(0) === "create_depression") {
        $("#patient_history_tab").trigger("click"),
            $("#ph_mentalhealth_tab").trigger("click"),
            $("#mh-depression").trigger("click"),
            localStorage.clear(),
            localStorage.removeItem("create_depression");
    }
});
//depression-edit
$(document).ready(function () {
    if (localStorage.key(0) === "edit_depression") {
        $("#patient_history_tab").trigger("click"),
            $("#ph_mentalhealth_tab").trigger("click"),
            $("#mh-depression").trigger("click"),
            localStorage.clear(),
            localStorage.removeItem("edit_depression");
    }
});
//depression-back
$(document).ready(function () {
    if (localStorage.key(0) === "back_depression") {
        $("#patient_history_tab").trigger("click"),
            $("#ph_mentalhealth_tab").trigger("click"),
            $("#mh-depression").trigger("click"),
            localStorage.removeItem("back_depression"),
            localStorage.clear();
    }
});
