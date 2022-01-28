    $("#clinical_rules tr td:contains('CD4Count')").each(function () {
        var cd4count_greater = $(this).siblings('td.greater').css("background-color", "#FFFF66");

        var cd4count = $('#bloodtest th:contains("CD4 count")').eq(0).index();
        $("#bloodtest tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + cd4count + ")").filter(function () {
                return parseFloat($(this).text(), 10) > parseFloat(cd4count_greater.text());
            }).addClass('max');
        });

    });
    $("#clinical_rules tr td:contains('CD4Count')").each(function () {
        var cd4count_less = $(this).siblings('td.less').css("background-color", "#FFFF66");

        var cd4count = $('#bloodtest th:contains("CD4 count")').eq(0).index();
        $("#bloodtest tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + cd4count + ")").filter(function () {
                return parseFloat($(this).text(), 10) < parseFloat(cd4count_less.text());
            }).addClass('min');
        });
    });

$("#clinical_rules tr td:contains('CD4Percentage')").each(function () {
        var cd4percentage_greater = $(this).siblings('td.greater').css("background-color", "#FFFF66");

        var cd4percentage = $('#bloodtest th:contains("CD4 percentage")').eq(0).index();
        $("#bloodtest tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + cd4percentage + ")").filter(function () {
                return parseFloat($(this).text(), 10) > parseFloat(cd4percentage_greater.text());
            }).addClass('max');
        });

    });
    $("#clinical_rules tr td:contains('CD4Percentage')").each(function () {
        var cd4percentage_less = $(this).siblings('td.less').css("background-color", "#FFFF66");

        var cd4percentage = $('#bloodtest th:contains("CD4 percentage")').eq(0).index();
        $("#bloodtest tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + cd4percentage + ")").filter(function () {
                return parseFloat($(this).text(), 10) < parseFloat(cd4percentage_less.text());
            }).addClass('min');
        });
    });

$("#clinical_rules tr td:contains('viralLoad')").each(function () {
        var viralload_greater = $(this).siblings('td.greater').css("background-color", "#FFFF66");

        var viralload = $('#bloodtest th:contains("Viral load")').eq(0).index();
        $("#bloodtest tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + viralload + ")").filter(function () {
                return parseFloat($(this).text(), 10) > parseFloat(viralload_greater.text());
            }).addClass('max');
        });

    });
    $("#clinical_rules tr td:contains('viralLoad')").each(function () {
        var viralload_less = $(this).siblings('td.less').css("background-color", "#FFFF66");

        var viralload = $('#bloodtest th:contains("Viral load")').eq(0).index();
        $("#bloodtest tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + viralload + ")").filter(function () {
                return parseFloat($(this).text(), 10) < parseFloat(viralload_less.text());
            }).addClass('min');
        });
    });
