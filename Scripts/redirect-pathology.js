    //pathology>createNew
    $(document).ready(function () {
            if (localStorage.key(0) === "create_pathologyCreate") {
        //var key = localStorage.key(0);
        //alert(key);
        $("#pathology_tab").trigger("click");
    localStorage.clear();
    localStorage.removeItem("create_pathologyCreate");
}
});
//pathology>back
        $(document).ready(function () {
            if (localStorage.key(0) === "back_pathologyCreate") {
        //var key = localStorage.key(0);
        //alert(key);
        $("#pathology_tab").trigger("click");
    localStorage.clear();
    localStorage.removeItem("back_pathologyCreate");
}
});
//pathology>general
        $(document).ready(function () {
            if (localStorage.key(0) === "back_general") {
        //var key = localStorage.key(0);
        //alert(key);
        $("#pathology_tab").trigger("click");
    localStorage.clear();
    localStorage.removeItem("back_general");
}
});
//pathology>hyperlipidaemia
        $(document).ready(function () {
            if (localStorage.key(0) === "back_hyperlipdaemia") {
        $("#pathology_tab").trigger("click");
    $("#hyperlipidaemia_tab").trigger("click");
    localStorage.clear();
    localStorage.removeItem("back_hyperlipdaemia");
}
});
//pathology>diabetes
        $(document).ready(function () {
            if (localStorage.key(0) === "back_diabetes") {
        $("#pathology_tab").trigger("click");
    $("#diabetes_tab").trigger("click");
    localStorage.clear();
    localStorage.removeItem("back_diabetes");
}
});
//pathology>hiv
        $(document).ready(function () {
            if (localStorage.key(0) === "back_hiv") {
        $("#pathology_tab").trigger("click");
    $("#hiv_tab").trigger("click");
    localStorage.clear();
    localStorage.removeItem("back_hiv");
}
});
