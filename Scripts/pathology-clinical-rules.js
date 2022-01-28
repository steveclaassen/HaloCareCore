// HCare-974
if (pf_array.indexOf('CD4 count') > -1) {
    $("#clinical_rules tr td:contains('CD4Count')").each(function () {
        var cd4count_greater = $(this).siblings('td.greater').css("background-color", "#FFFF66");

        //var n = parseFloat(bmi_greater.text());

        var cd4count = $('#pathology_results th:contains("CD4 count")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + cd4count + ")").filter(function () {
                return parseFloat($(this).text(), 10) > parseFloat(cd4count_greater.text());
            }).addClass('max');
        });

    });
    $("#clinical_rules tr td:contains('CD4Count')").each(function () {
        var cd4count_less = $(this).siblings('td.less').css("background-color", "#FFFF66");

        var cd4count = $('#pathology_results th:contains("CD4 count")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + cd4count + ")").filter(function () {
                return parseFloat($(this).text(), 10) < parseFloat(cd4count_less.text());
            }).addClass('min');
        });
    });
}
if (pf_array.indexOf('CD4 percentage') > -1) {
    $("#clinical_rules tr td:contains('CD4Percentage')").each(function () {
        var cd4percentage_greater = $(this).siblings('td.greater').css("background-color", "#FFFF66");

        var cd4percentage = $('#pathology_results th:contains("CD4 percentage")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + cd4percentage + ")").filter(function () {
                return parseFloat($(this).text(), 10) > parseFloat(cd4percentage_greater.text());
            }).addClass('max');
        });

    });
    $("#clinical_rules tr td:contains('CD4Percentage')").each(function () {
        var cd4percentage_less = $(this).siblings('td.less').css("background-color", "#FFFF66");

        var cd4percentage = $('#pathology_results th:contains("CD4 percentage")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + cd4percentage + ")").filter(function () {
                return parseFloat($(this).text(), 10) < parseFloat(cd4percentage_less.text());
            }).addClass('min');
        });
    });
}
if (pf_array.indexOf('Viral load') > -1) {
    $("#clinical_rules tr td:contains('viralLoad')").each(function () {
        var viralload_greater = $(this).siblings('td.greater').css("background-color", "#FFFF66");

        var viralload = $('#pathology_results th:contains("Viral load")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + viralload + ")").filter(function () {
                return parseFloat($(this).text(), 10) > parseFloat(viralload_greater.text());
            }).addClass('max');
        });

    });
    $("#clinical_rules tr td:contains('viralLoad')").each(function () {
        var viralload_less = $(this).siblings('td.less').css("background-color", "#FFFF66");

        var viralload = $('#pathology_results th:contains("Viral load")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + viralload + ")").filter(function () {
                return parseFloat($(this).text(), 10) < parseFloat(viralload_less.text());
            }).addClass('min');
        });
    });
}
if (pf_array.indexOf('Haemoglobin') > -1) {
    $("#clinical_rules tr td:contains('haemoglobin')").each(function () {
        var haemoglobin_greater = $(this).siblings('td.greater').css("background-color", "#FFFF66");

        var haemoglobin = $('#pathology_results th:contains("Haemoglobin")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + haemoglobin + ")").filter(function () {
                return parseFloat($(this).text(), 10) > parseFloat(haemoglobin_greater.text());
            }).addClass('max');
        });

    });
    $("#clinical_rules tr td:contains('haemoglobin')").each(function () {
        var haemoglobin_less = $(this).siblings('td.less').css("background-color", "#FFFF66");

        var haemoglobin = $('#pathology_results th:contains("Haemoglobin")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + haemoglobin + ")").filter(function () {
                return parseFloat($(this).text(), 10) < parseFloat(haemoglobin_less.text());
            }).addClass('min');
        });
    });
}
if (pf_array.indexOf('Bilirubin') > -1) {
    $("#clinical_rules tr td:contains('bilirubin')").each(function () {
        var bilirubin_greater = $(this).siblings('td.greater').css("background-color", "#FFFF66");

        var bilirubin = $('#pathology_results th:contains("Bilirubin")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + bilirubin + ")").filter(function () {
                return parseFloat($(this).text(), 10) > parseFloat(bilirubin_greater.text());
            }).addClass('max');
        });

    });
    $("#clinical_rules tr td:contains('bilirubin')").each(function () {
        var bilirubin_less = $(this).siblings('td.less').css("background-color", "#FFFF66");

        var bilirubin = $('#pathology_results th:contains("Bilirubin")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + bilirubin + ")").filter(function () {
                return parseFloat($(this).text(), 10) < parseFloat(bilirubin_less.text());
            }).addClass('min');
        });
    });
}
if (pf_array.indexOf('Total Cholestrol') > -1) {
    $("#clinical_rules tr td:contains('totalCholestrol')").each(function () {
        var totalCholestrol_greater = $(this).siblings('td.greater').css("background-color", "#FFFF66");

        var totalCholestrol = $('#pathology_results th:contains("Total Cholestrol")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + totalCholestrol + ")").filter(function () {
                return parseFloat($(this).text(), 10) > parseFloat(totalCholestrol_greater.text());
            }).addClass('max');
        });

    });
    $("#clinical_rules tr td:contains('totalCholestrol')").each(function () {
        var totalCholestrol_less = $(this).siblings('td.less').css("background-color", "#FFFF66");

        var totalCholestrol = $('#pathology_results th:contains("Total Cholestrol")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + totalCholestrol + ")").filter(function () {
                return parseFloat($(this).text(), 10) < parseFloat(totalCholestrol_less.text());
            }).addClass('min');
        });
    });
}
if (pf_array.indexOf('HDL') > -1) {
    $("#clinical_rules tr td:contains('hdl')").each(function () {
        var hdl_greater = $(this).siblings('td.greater').css("background-color", "#FFFF66");

        var hdl = $('#pathology_results th:contains("HDL")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + hdl + ")").filter(function () {
                return parseFloat($(this).text(), 10) > parseFloat(hdl_greater.text());
            }).addClass('max');
        });

    });
    $("#clinical_rules tr td:contains('hdl')").each(function () {
        var hdl_less = $(this).siblings('td.less').css("background-color", "#FFFF66");

        var hdl = $('#pathology_results th:contains("HDL")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + hdl + ")").filter(function () {
                return parseFloat($(this).text(), 10) < parseFloat(hdl_less.text());
            }).addClass('min');
        });
    });
}
if (pf_array.indexOf('LDL') > -1) {
    $("#clinical_rules tr td:contains('ldl')").each(function () {
        var ldl_greater = $(this).siblings('td.greater').css("background-color", "#FFFF66");

        var ldl = $('#pathology_results th:contains("LDL")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + ldl + ")").filter(function () {
                return parseFloat($(this).text(), 10) > parseFloat(ldl_greater.text());
            }).addClass('max');
        });

    });
    $("#clinical_rules tr td:contains('ldl')").each(function () {
        var ldl_less = $(this).siblings('td.less').css("background-color", "#FFFF66");

        var ldl = $('#pathology_results th:contains("LDL")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + ldl + ")").filter(function () {
                return parseFloat($(this).text(), 10) < parseFloat(ldl_less.text());
            }).addClass('min');
        });
    });
}
if (pf_array.indexOf('Triglycerides') > -1) {
    $("#clinical_rules tr td:contains('triglycerides')").each(function () {
        var triglycerides_greater = $(this).siblings('td.greater').css("background-color", "#FFFF66");

        var triglycerides = $('#pathology_results th:contains("Triglycerides")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + triglycerides + ")").filter(function () {
                return parseFloat($(this).text(), 10) > parseFloat(triglycerides_greater.text());
            }).addClass('max');
        });

    });
    $("#clinical_rules tr td:contains('triglycerides')").each(function () {
        var triglycerides_less = $(this).siblings('td.less').css("background-color", "#FFFF66");

        var triglycerides = $('#pathology_results th:contains("Triglycerides")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + triglycerides + ")").filter(function () {
                return parseFloat($(this).text(), 10) < parseFloat(triglycerides_less.text());
            }).addClass('min');
        });
    });
}
if (pf_array.indexOf('Glucose') > -1) {
    $("#clinical_rules tr td:contains('glucose')").each(function () {
        var glucose_greater = $(this).siblings('td.greater').css("background-color", "#FFFF66");

        var glucose = $('#pathology_results th:contains("Glucose")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + glucose + ")").filter(function () {
                return parseFloat($(this).text(), 10) > parseFloat(glucose_greater.text());
            }).addClass('max');
        });

    });
    $("#clinical_rules tr td:contains('glucose')").each(function () {
        var glucose_less = $(this).siblings('td.less').css("background-color", "#FFFF66");

        var glucose = $('#pathology_results th:contains("Glucose")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + glucose + ")").filter(function () {
                return parseFloat($(this).text(), 10) < parseFloat(glucose_less.text());
            }).addClass('min');
        });
    });
}
if (pf_array.indexOf('HbA1c') > -1) {
    $("#clinical_rules tr td:contains('hba1c')").each(function () {
        var hba1c_greater = $(this).siblings('td.greater').css("background-color", "#FFFF66");

        var hba1c = $('#pathology_results th:contains("HbA1c")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + hba1c + ")").filter(function () {
                return parseFloat($(this).text(), 10) > parseFloat(hba1c_greater.text());
            }).addClass('max');
        });

    });
    $("#clinical_rules tr td:contains('hba1c')").each(function () {
        var hba1c_less = $(this).siblings('td.less').css("background-color", "#FFFF66");

        var hba1c = $('#pathology_results th:contains("HbA1c")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + hba1c + ")").filter(function () {
                return parseFloat($(this).text(), 10) < parseFloat(hba1c_less.text());
            }).addClass('min');
        });
    });
}
if (pf_array.indexOf('ALT') > -1) {
    $("#clinical_rules tr td:contains('alt')").each(function () {
        var alt_greater = $(this).siblings('td.greater').css("background-color", "#FFFF66");

        var alt = $('#pathology_results th:contains("ALT")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + alt + ")").filter(function () {
                return parseFloat($(this).text(), 10) > parseFloat(alt_greater.text());
            }).addClass('max');
        });

    });
    $("#clinical_rules tr td:contains('alt')").each(function () {
        var alt_less = $(this).siblings('td.less').css("background-color", "#FFFF66");

        var alt = $('#pathology_results th:contains("ALT")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + alt + ")").filter(function () {
                return parseFloat($(this).text(), 10) < parseFloat(alt_less.text());
            }).addClass('min');
        });
    });
}
if (pf_array.indexOf('AST') > -1) {
    $("#clinical_rules tr td:contains('ast')").each(function () {
        var ast_greater = $(this).siblings('td.greater').css("background-color", "#FFFF66");

        var ast = $('#pathology_results th:contains("AST")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + ast + ")").filter(function () {
                return parseFloat($(this).text(), 10) > parseFloat(ast_greater.text());
            }).addClass('max');
        });

    });
    $("#clinical_rules tr td:contains('ast')").each(function () {
        var ast_less = $(this).siblings('td.less').css("background-color", "#FFFF66");

        var ast = $('#pathology_results th:contains("AST")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + ast + ")").filter(function () {
                return parseFloat($(this).text(), 10) < parseFloat(ast_less.text());
            }).addClass('min');
        });
    });
}
if (pf_array.indexOf('Urea') > -1) {
    $("#clinical_rules tr td:contains('urea')").each(function () {
        var urea_greater = $(this).siblings('td.greater').css("background-color", "#FFFF66");

        var urea = $('#pathology_results th:contains("Urea")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + urea + ")").filter(function () {
                return parseFloat($(this).text(), 10) > parseFloat(urea_greater.text());
            }).addClass('max');
        });

    });
    $("#clinical_rules tr td:contains('urea')").each(function () {
        var urea_less = $(this).siblings('td.less').css("background-color", "#FFFF66");

        var urea = $('#pathology_results th:contains("Urea")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + urea + ")").filter(function () {
                return parseFloat($(this).text(), 10) < parseFloat(urea_less.text());
            }).addClass('min');
        });
    });
}
if (pf_array.indexOf('Creatinine') > -1) {
    $("#clinical_rules tr td:contains('creatinine')").each(function () {
        var creatinine_greater = $(this).siblings('td.greater').css("background-color", "#FFFF66");

        var creatinine = $('#pathology_results th:contains("Creatinine")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + creatinine + ")").filter(function () {
                return parseFloat($(this).text(), 10) > parseFloat(creatinine_greater.text());
            }).addClass('max');
        });

    });
    $("#clinical_rules tr td:contains('creatinine')").each(function () {
        var creatinine_less = $(this).siblings('td.less').css("background-color", "#FFFF66");

        var creatinine = $('#pathology_results th:contains("Creatinine")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + creatinine + ")").filter(function () {
                return parseFloat($(this).text(), 10) < parseFloat(creatinine_less.text());
            }).addClass('min');
        });
    });
}
if (pf_array.indexOf('eGFR') > -1) {
    $("#clinical_rules tr td:contains('eGfr')").each(function () {
        var eGfr_greater = $(this).siblings('td.greater').css("background-color", "#FFFF66");

        var eGfr = $('#pathology_results th:contains("eGFR")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + eGfr + ")").filter(function () {
                return parseFloat($(this).text(), 10) > parseFloat(eGfr_greater.text());
            }).addClass('max');
        });

    });
    $("#clinical_rules tr td:contains('eGfr')").each(function () {
        var eGfr_less = $(this).siblings('td.less').css("background-color", "#FFFF66");

        var eGfr = $('#pathology_results th:contains("eGFR")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + eGfr + ")").filter(function () {
                return parseFloat($(this).text(), 10) < parseFloat(eGfr_less.text());
            }).addClass('min');
        });
    });
}
if (pf_array.indexOf('U Albumin to Creat ratio') > -1) {
    $("#clinical_rules tr td:contains('mauCreatRatio')").each(function () {
        var mauCreatRatio_greater = $(this).siblings('td.greater').css("background-color", "#FFFF66");

        var mauCreatRatio = $('#pathology_results th:contains("U Albumin to Creat ratio")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + mauCreatRatio + ")").filter(function () {
                return parseFloat($(this).text(), 10) > parseFloat(mauCreatRatio_greater.text());
            }).addClass('max');
        });

    });
    $("#clinical_rules tr td:contains('mauCreatRatio')").each(function () {
        var mauCreatRatio_less = $(this).siblings('td.less').css("background-color", "#FFFF66");

        var mauCreatRatio = $('#pathology_results th:contains("U Albumin to Creat ratio")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + mauCreatRatio + ")").filter(function () {
                return parseFloat($(this).text(), 10) < parseFloat(mauCreatRatio_less.text());
            }).addClass('min');
        });
    });
}
if (pf_array.indexOf('Sistalic BP') > -1) {
    $("#clinical_rules tr td:contains('systolicBP')").each(function () {
        var systolicBP_greater = $(this).siblings('td.greater').css("background-color", "#FFFF66");

        var systolicBP = $('#pathology_results th:contains("Sistalic BP")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + systolicBP + ")").filter(function () {
                return parseFloat($(this).text(), 10) > parseFloat(systolicBP_greater.text());
            }).addClass('max');
        });

    });
    $("#clinical_rules tr td:contains('systolicBP')").each(function () {
        var systolicBP_less = $(this).siblings('td.less').css("background-color", "#FFFF66");

        var systolicBP = $('#pathology_results th:contains("Sistalic BP")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + systolicBP + ")").filter(function () {
                return parseFloat($(this).text(), 10) < parseFloat(systolicBP_less.text());
            }).addClass('min');
        });
    });
}
if (pf_array.indexOf('Diastalic BP') > -1) {
    $("#clinical_rules tr td:contains('diastolicBP')").each(function () {
        var diastolicBP_greater = $(this).siblings('td.greater').css("background-color", "#FFFF66");

        var diastolicBP = $('#pathology_results th:contains("Diastalic BP")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + diastolicBP + ")").filter(function () {
                return parseFloat($(this).text(), 10) > parseFloat(diastolicBP_greater.text());
            }).addClass('max');
        });

    });
    $("#clinical_rules tr td:contains('diastolicBP')").each(function () {
        var diastolicBP_less = $(this).siblings('td.less').css("background-color", "#FFFF66");

        var diastolicBP = $('#pathology_results th:contains("Diastalic BP")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + diastolicBP + ")").filter(function () {
                return parseFloat($(this).text(), 10) < parseFloat(diastolicBP_less.text());
            }).addClass('min');
        });
    });
}
if (pf_array.indexOf('FEV1') > -1) {
    $("#clinical_rules tr td:contains('FEV1')").each(function () {
        var FEV1_greater = $(this).siblings('td.greater').css("background-color", "#FFFF66");

        var FEV1 = $('#pathology_results th:contains("FEV1")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + FEV1 + ")").filter(function () {
                return parseFloat($(this).text(), 10) > parseFloat(FEV1_greater.text());
            }).addClass('max');
        });

    });
    $("#clinical_rules tr td:contains('FEV1')").each(function () {
        var FEV1_less = $(this).siblings('td.less').css("background-color", "#FFFF66");

        var FEV1 = $('#pathology_results th:contains("FEV1")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + FEV1 + ")").filter(function () {
                return parseFloat($(this).text(), 10) < parseFloat(FEV1_less.text());
            }).addClass('min');
        });
    });
}
if (pf_array.indexOf('Eosinophylia') > -1) {
    $("#clinical_rules tr td:contains('Eosinophylia')").each(function () {
        var Eosinophylia_greater = $(this).siblings('td.greater').css("background-color", "#FFFF66");

        var Eosinophylia = $('#pathology_results th:contains("Eosinophylia")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + Eosinophylia + ")").filter(function () {
                return parseFloat($(this).text(), 10) > parseFloat(Eosinophylia_greater.text());
            }).addClass('max');
        });

    });
    $("#clinical_rules tr td:contains('Eosinophylia')").each(function () {
        var Eosinophylia_less = $(this).siblings('td.less').css("background-color", "#FFFF66");

        var Eosinophylia = $('#pathology_results th:contains("Eosinophylia")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + Eosinophylia + ")").filter(function () {
                return parseFloat($(this).text(), 10) < parseFloat(Eosinophylia_less.text());
            }).addClass('min');
        });
    });
}
if (pf_array.indexOf('U Creatinine') > -1) {
    $("#clinical_rules tr td:contains('ucreatinine')").each(function () {
        var ucreatinine_greater = $(this).siblings('td.greater').css("background-color", "#FFFF66");

        var ucreatinine = $('#pathology_results th:contains("U Creatinine")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + ucreatinine + ")").filter(function () {
                return parseFloat($(this).text(), 10) > parseFloat(ucreatinine_greater.text());
            }).addClass('max');
        });

    });
    $("#clinical_rules tr td:contains('ucreatinine')").each(function () {
        var ucreatinine_less = $(this).siblings('td.less').css("background-color", "#FFFF66");

        var ucreatinine = $('#pathology_results th:contains("U Creatinine")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + ucreatinine + ")").filter(function () {
                return parseFloat($(this).text(), 10) < parseFloat(ucreatinine_less.text());
            }).addClass('min');
        });
    });
}
if (pf_array.indexOf('S Albumin') > -1) {
    $("#clinical_rules tr td:contains('salbumin')").each(function () {
        var salbumin_greater = $(this).siblings('td.greater').css("background-color", "#FFFF66");

        var salbumin = $('#pathology_results th:contains("S Albumin")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + salbumin + ")").filter(function () {
                return parseFloat($(this).text(), 10) > parseFloat(salbumin_greater.text());
            }).addClass('max');
        });

    });
    $("#clinical_rules tr td:contains('salbumin')").each(function () {
        var salbumin_less = $(this).siblings('td.less').css("background-color", "#FFFF66");

        var salbumin = $('#pathology_results th:contains("S Albumin")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + salbumin + ")").filter(function () {
                return parseFloat($(this).text(), 10) < parseFloat(salbumin_less.text());
            }).addClass('min');
        });
    });
}
if (pf_array.indexOf('U Albuminuria') > -1) {
    $("#clinical_rules tr td:contains('ualbuminuria')").each(function () {
        var ualbuminuria_greater = $(this).siblings('td.greater').css("background-color", "#FFFF66");

        var ualbuminuria = $('#pathology_results th:contains("U Albuminuria")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + ualbuminuria + ")").filter(function () {
                return parseFloat($(this).text(), 10) > parseFloat(ualbuminuria_greater.text());
            }).addClass('max');
        });

    });
    $("#clinical_rules tr td:contains('ualbuminuria')").each(function () {
        var ualbuminuria_less = $(this).siblings('td.less').css("background-color", "#FFFF66");

        var ualbuminuria = $('#pathology_results th:contains("U Albuminuria")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + ualbuminuria + ")").filter(function () {
                return parseFloat($(this).text(), 10) < parseFloat(ualbuminuria_less.text());
            }).addClass('min');
        });
    });
}
if (pf_array.indexOf('HIV Elisa') > -1) {
    $("#clinical_rules tr td:contains('hivEliza')").each(function () {
        var hivEliza_greater = $(this).siblings('td.greater').css("background-color", "#FFFF66");

        var hivEliza = $('#pathology_results th:contains("HIV Elisa")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + hivEliza + ")").filter(function () {
                return parseFloat($(this).text(), 10) > parseFloat(hivEliza_greater.text());
            }).addClass('max');
        });

    });
    $("#clinical_rules tr td:contains('hivEliza')").each(function () {
        var hivEliza_less = $(this).siblings('td.less').css("background-color", "#FFFF66");

        var hivEliza = $('#pathology_results th:contains("HIV Elisa")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + hivEliza + ")").filter(function () {
                return parseFloat($(this).text(), 10) < parseFloat(hivEliza_less.text());
            }).addClass('min');
        });
    });
}
if (pf_array.indexOf('Normal GTT') > -1) {
    $("#clinical_rules tr td:contains('normGtt')").each(function () {
        var normGtt_greater = $(this).siblings('td.greater').css("background-color", "#FFFF66");

        var normGtt = $('#pathology_results th:contains("Normal GTT")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + normGtt + ")").filter(function () {
                return parseFloat($(this).text(), 10) > parseFloat(normGtt_greater.text());
            }).addClass('max');
        });

    });
    $("#clinical_rules tr td:contains('normGtt')").each(function () {
        var normGtt_less = $(this).siblings('td.less').css("background-color", "#FFFF66");

        var normGtt = $('#pathology_results th:contains("Normal GTT")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + normGtt + ")").filter(function () {
                return parseFloat($(this).text(), 10) < parseFloat(normGtt_less.text());
            }).addClass('min');
        });
    });
}
if (pf_array.indexOf('Abnormal GTT') > -1) {
    $("#clinical_rules tr td:contains('abnGtt')").each(function () {
        var abnGtt_greater = $(this).siblings('td.greater').css("background-color", "#FFFF66");

        var abnGtt = $('#pathology_results th:contains("Abnormal GTT")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + abnGtt + ")").filter(function () {
                return parseFloat($(this).text(), 10) > parseFloat(abnGtt_greater.text());
            }).addClass('max');
        });

    });
    $("#clinical_rules tr td:contains('abnGtt')").each(function () {
        var abnGtt_less = $(this).siblings('td.less').css("background-color", "#FFFF66");

        var abnGtt = $('#pathology_results th:contains("Abnormal GTT")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + abnGtt + ")").filter(function () {
                return parseFloat($(this).text(), 10) < parseFloat(abnGtt_less.text());
            }).addClass('min');
        });
    });
}
if (pf_array.indexOf('BMI') > -1) {
    $("#clinical_rules tr td:contains('bmi')").each(function () {
        var bmi_greater = $(this).siblings('td.greater').css("background-color", "#FFFF66");

        var bmi = $('#pathology_results th:contains("BMI")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + bmi + ")").filter(function () {
                return parseFloat($(this).text(), 10) > parseFloat(bmi_greater.text());
            }).addClass('max');
        });

    });
    $("#clinical_rules tr td:contains('bmi')").each(function () {
        var bmi_less = $(this).siblings('td.less').css("background-color", "#FFFF66");

        var bmi = $('#pathology_results th:contains("BMI")').eq(0).index();
        $("#pathology_results tr").each(function (rowIndex, row) {
            $(row).find("td:eq(" + bmi + ")").filter(function () {
                return parseFloat($(this).text(), 10) < parseFloat(bmi_less.text());
            }).addClass('min');
        });
    });
}
