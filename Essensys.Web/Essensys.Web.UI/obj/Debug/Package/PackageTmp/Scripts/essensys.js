/* Essensys.js
* 
* 20140428
*/
var cptalarme = 500;
var dec = "17px";
var ar = "";

$(document).ready(function () {
    $.each($(".esys-zone input[checked=checked]"), function (i, o) {
        $(o).attr('data-initial', 'true');
    });
    $(".esys-zone").css("padding-top", dec);
    $("#openvol").bind("click", function (e) {
        e.preventDefault();
        var ct = $(this).parent().parent();
        $.each($(ct).find("input[value=1]"), function (iv, ov) {
            if (!$(ov).is(":checked")) {
                if ($(ov).attr('id') != 'store')
                    $(ov).click();
            }
            else {
                if ($(ov).attr('id') != 'store')
                    $(ov).prop("checked", false);
            }
        });
    });
    $("#closevol").bind("click", function (e) {
        e.preventDefault();
        var ct = $(this).parent().parent();
        $.each($(ct).find("input[value=0]"), function (iv, ov) {
            if (!$(ov).is(":checked")) {
                if ($(ov).attr('id') != 'store')
                    $(ov).click();
            }
            else {
                if ($(ov).attr('id') != 'store')
                    $(ov).prop("checked", false);
            }
        });
    });
    $("#openeclind,#openecldir").bind("click", function (e) {
        e.preventDefault();
        var ct = $(this).parent().parent();
        $.each($(ct).find("input[value=1]"), function (iv, ov) {
            if (!$(ov).is(":checked")) {
                $(ov).click();
            }
            else {
                $(ov).prop("checked", false);
            }
        });
    });
    $("#closeeclind,#closeecldir").bind("click", function (e) {
        e.preventDefault();
        var ct = $(this).parent().parent();
        $.each($(ct).find("input[value=0]"), function (iv, ov) {
            if (!$(ov).is(":checked")) {
                $(ov).click();
            }
            else {
                $(ov).prop("checked", false);
            }
        });
    });
    $('#phone').bind('change', function () {
        if ($(this).val() != '') {
            savephone($(this));
        }
    });

    $('#sendmail').bind('click', function () {
        if ($("#phone").val() != '') {
            savephone($(this));
        }
    });

    // Test attente box
    if ($('#esys-valid').attr('data-waitbox') == 'true') {
        $.colorbox({
            html: "<div class='esys-wait' style='width:350px'>Synchronisation avec Essensys<br/>Veuillez patienter</div>",
            overlayClose: false,
            escKey: false,
            width: "400px",
            height: "400px"
        });
        $('input').attr('disabled', 'disabled');
        $.post("/Home/WaitBox", function (data) {
            if (data.success) {
                if (data.newversion) {
                    if (data.newversionnotpossible) {
                        $.colorbox({
                            html: "<div class='esys-versiondesc'>Votre Essensys doit être mis à jour. Cependant, il est impossible de lancer cette mise à jour car la maison est sous alarme ou l'écran de contrôle Essensys n'est pas en veille. Vérifiez que le système d'alarme est bien désactivé et que l'écran de contrôle est en veille avant de réessayer.</div><div style='text-align:center;font-weight:bold'><a href='#' class='esys-versionlaunch'>Réessayer</a><br/><br/><a href='#' class='esys-versionstop'>Je recommencerai plus tard</a></div></div>",
                            overlayClose: false,
                            escKey: false,
                            width: "400px",
                            height: "400px"
                        });
                        $(".esys-versionlaunch").bind('click', function () {
                            window.location = "/";
                        });
                        $(".esys-versionstop").bind('click', function () {
                            allowform();
                            $.colorbox.close();
                            refreshzones(data);
                        });
                    }
                    else {
                        $.colorbox({
                            html: "<div class='esys-version'>Votre Essensys doit être mis à jour.</div><div class='esys-description' style='display:none'><div>La mise à jour durera 15 minutes environ et ne démarrera que si votre système d\'alarme n\'est pas activé et que l\'écran de contrôle Essensys est en veille. Il est recommandé d\'effectuer cette mise à jour en journée car elle occasionnera un redémarrage de votre Essensys, le rendant inutilisable et vous privant d\'éclairage pendant 30 secondes environ. Si pour une raison ou pour une autre la procédure de mise à jour devait être interrompue avant la fin, le système continuera simplement à fonctionner avec la version logicielle actuelle, et la mise à jour vous sera à nouveau proposée lors de votre prochaine connection.</div><div class='esys-versiondesc'>" + data.versiondesc + "</div></div><br/><div style='text-align:center;font-weight:bold'><a href='#' class='esys-versionstop'>Plus tard</a><br/><br/><a href='#' class='esys-versionmore'>En savoir plus et reporter</a><br/><br/><a href='#' class='esys-versionlaunch'>Mettre à jour</a></div></div>",
                            overlayClose: false,
                            escKey: false
                        });
                        $(".esys-versionstop").bind('click', function () {
                            allowform();
                            $.colorbox.close();
                            refreshzones(data);
                        });
                        $(".esys-versionmore").bind("click", function () {
                            $.colorbox.resize({ width: "400px", height: "400px" });
                            $(".esys-version").hide();
                            $(".esys-versionstop").html("Fermer");
                            $(".esys-description").show();
                            $(".esys-versionlaunch").hide();
                            $(".esys-versionmore").hide();
                        });
                        $(".esys-versionlaunch").bind('click', function () {
                            $.post("/Home/WaitBox", function (datav) {
                                if (datav.success) {
                                    if (datav.newversion) {
                                        if (datav.newversionnotpossible) {
                                            $.colorbox({
                                                html: "<div class='esys-versiondesc'>Votre Essensys doit être mis à jour. Cependant, il est impossible de lancer cette mise à jour car la maison est sous alarme ou l'écran de contrôle Essensys n'est pas en veille. Vérifiez que le système d'alarme est bien désactivé et que l'écran de contrôle est en veille avant de réessayer.</div><div style='text-align:center;font-weight:bold'><a href='#' class='esys-versionlaunch'>Réessayer</a><br/><br/><a href='#' class='esys-versionstop'>Je recommencerai plus tard</a></div></div>",
                                                overlayClose: false,
                                                escKey: false,
                                                width: "400px",
                                                height: "400px"
                                            });
                                            $(".esys-versionlaunch").bind('click', function () {
                                                window.location = "/";
                                            });
                                            $(".esys-versionstop").bind('click', function () {
                                                allowform();
                                                $.colorbox.close();
                                                refreshzones(datav);
                                            });
                                        }
                                        else {
                                            $.colorbox({
                                                html: "<div style='overflow:hidden'><div class='esys-wait'>Téléchargement en cours<br/>Veuillez patienter</div><div class='progress'></div></div>",
                                                overlayClose: false,
                                                escKey: false,
                                                height: '300px'
                                            });
                                            $('.progress').progressbar({ max: data.versionsize, value: 0 });
                                            download(data);
                                        }
                                    }
                                }
                                else {
                                    $('.esys-wait').html("Votre boitier ne répond pas : " + datav.message + "<br/><div style='text-align:center'><a href='#'>Réessayer</a><br/><a href='/Account/UpdateMyInfos' style='margin-top:0px'>Accédez à vos informations personnelles</a></div>");
                                    $('.esys-wait a').bind('click', function () {
                                        $.colorbox.close();
                                        window.location = "/";
                                    });
                                    $('.esys-wait').css('background', 'none');
                                }
                            });
                        });
                    }
                }
                else {
                    allowform();
                    $.colorbox.close();
                    refreshzones(data);
                }
            }
            else {
                if (data.step == 0) {
                    $('.esys-wait').html(data.message + "<br/><div style='text-align:center'><a href='#'>Réessayer</a><br/><a href='/Account/UpdateMyInfos' style='margin-top:0px'>Accédez à vos informations personnelles</a></div>");
                    $('.esys-wait a').bind('click', function () {
                        $.colorbox.close();
                        window.location = "/";
                    });
                    $('.esys-wait').css('background', 'none');
                }
                else {
                    $.colorbox({
                        html: "<div style='overflow:hidden'><div class='esys-wait'>Téléchargement en cours<br/>Veuillez patienter</div><div class='progress'></div></div>",
                        overlayClose: false,
                        escKey: false,
                        height: '300px'
                    });
                    $('.progress').progressbar({ max: data.size, value: data.step });
                    download(data);
                }
            }
        });
    }

    if ($('form').attr('action') == '/Account/UpdateMyInfos') {
        $('form').attr('autocomplete', 'off');
    }

    // Téléphone
    $.each($('.phonelist'), function (i1, o1) {
        $.getJSON('/Phone/ListPhone', function (data) {
            if (data.success && data.phones.length > 0) {
                $('#phone').val(data.phones[0].Phone);
                $('#phone').attr('data-id', data.phones[0].Id);
                if (data.phones[0].Sendmail) {
                    $("#sendmail").attr("checked", "checked");
                }
                //registerphoneactions();
            }
        });
    });

    $(".esys-zone input[type=radio]").bind("click", function () {
        if ($(this).attr('data-initial') == 'true') {
            $(this).parent().parent().parent().find('span').hide();
            $(this).parent().parent().parent().css('padding-top', dec);
            $(this).parent().parent().removeClass('tovalidate');
        }
        else {
            $(this).parent().parent().parent().find('span').show();
            $(this).parent().parent().parent().css('padding-top', '0px');
            $(this).parent().parent().addClass('tovalidate');
        }
    });
    $(".esys-zone input[type=checkbox]").bind("click", function () {
        if ($("#volet").attr('checked') == 'checked' || $("#store").attr('checked') == 'checked') {
            $(this).parent().parent().parent().find('span').show();
            $(this).parent().parent().parent().css('padding-top', '0px');
            $(this).parent().parent().addClass('tovalidate');
        }
        else {
            $(this).parent().parent().parent().find('span').hide();
            $(this).parent().parent().parent().css('padding-top', dec);
            $(this).parent().parent().removeClass('tovalidate');
        }
    });
    $('#codealarme').bind("change", function () {
        if ($(this).val() != "" && $(this).val().length == 4) {
            $("#question").val("");
            allowalarme();
        } else {
            disallowalarme(true);
        }
    });
    $('#question').bind("change", function () {
        if ($(this).val() != "") {
            $("#codealarme").val("");
            $.post("/Account/TestResponse", { res: $(this).val() }, function (data) {
                if (data.success) {
                    if (data.responseisok)
                        allowalarme();
                    else
                        disallowalarme();
                }
                else {
                    if (data.redirect)
                        window.location = "/";
                    disallowalarme(true);
                }
            });
        }
    });
    $('#esys-undo, #esys-undo2').bind('click', function () {
        $("#question").val("");
        $("#codealarme").val("");
        $("input[data-initial=true]").click();
        disallowalarme(false);
        window.location = "/Account/Logout";
    });

    $('#esys-close').bind('click', function (e) {
        e.preventDefault();
        $.colorbox({ overlayClose: false, html: "<div class='esMessage'>Voulez-vous réellement clôturer le compte ?</div><input type='button' value='Oui' id='esys-yes' /><input type='button' value='Non' id='esys-no' />" });
        $('#esys-no').bind('click', function () {
            $.colorbox.close()
        });
        $('#esys-yes').bind('click', function () {
            $.post('/Account/CloseAccount', function (data) {
                if (data.success) {
                    window.location = '/Account/CloseMessage';
                }
                else {
                    $(".esMessage").html(data.message);
                }
            });
        });
    });
    $('#esys-valid,#esys-valid2').bind('click', function () {
        savephone($("#phone"), function () {
            $.colorbox({
                html: "<div class='esys-wait'>Synchronisation avec Essensys<br/>Veuillez patienter<div class='esys-waitstep'></div></div>",
                overlayClose: false,
                escKey: false
            });
            $('input').attr('disabled', 'disabled');

            // Encodage du formulaire
            doactions();
        });
    });
});

function downloadstep(){
    $.post("/Home/WaitVersion", function(data2){
        if (data2.success){
            if (!data2.hasfinished){
                $(".progress").progressbar( "option", "value", data2.step );
                setTimeout(downloadstep, 3000);
            }
            else{
                $('.esys-wait').html("Le téléchargement est terminé. Votre boitier se met à jour.<br/><a href='#'>Fermer l'application</a>");
                $('.esys-wait a').bind('click', function () {
                    $.colorbox.close();
                    window.location = "/Account/Logout";
                });
                $('.esys-wait').css('background', 'none');
            }
        }
        else{
            $('.esys-wait').html(data.message + "<br/><a href='#'>Réessayer</a>");
            $('.esys-wait a').bind('click', function () {
                $.colorbox.close();
                window.location = "/";
            });
            $('.esys-wait').css('background', 'none');
        } 
    });
};

function download(data){
    console.log("download");
    $.post("/Home/InitVersion", function(data2){
        console.log(data2);
        if (data2.success){
            setTimeout(downloadstep, 3000);
//            allowform();
//            $.colorbox.close();
//            refreshzones(data);
        }
        else{
            if (data2.nosession)
                window.location = "/";
            else{
                $('.esys-wait').html(data2.message + "<br/><a href='#'>Réessayer</a>");
                $('.esys-wait a').bind('click', function () {
                    $.colorbox.close();
                    window.location = "/";
                });
                $('.esys-wait').css('background', 'none');
            }
        }
    });
}

function doactions() {
    console.log("doactions");
    var newar = ($('#arrosagecontainer span').css('display') != 'none');
    var newal = ($('#alarmecontainer span').css('display') != 'none');
    var newcf = ($('#chauffagezjcontainer span').css('display') != 'none');
    var newcfzn = ($('#chauffagezncontainer span').css('display') != 'none');
    var newcfsdb1 = ($('#chauffagesdb1container span').css('display') != 'none');
    var newcfsdb2 = ($('#chauffagesdb2container span').css('display') != 'none');
    var newcm = ($('#cumuluscontainer span').css('display') != 'none');

    $(".esys-waitstep").html("Envoi des informations");
    ar = $('input:radio[name=alarme]:checked').val();
    var vals = {
        newar: newar,
        ar: $('input:radio[name=arrosage]:checked').val(),
        newal: newal,
        al: $('input:radio[name=alarme]:checked').val(),
        alresp: $("#question").val(),
        codealarme: $("#codealarme").val(),
        newcf: newcf,
        cfzj: $('input:radio[name=chauffagezj]:checked').val(),
        newcfzn: newcfzn,
        cfzn: $('input:radio[name=chauffagezn]:checked').val(),
        newcfsdb1: newcfsdb1,
        cfsdb1: $('input:radio[name=chauffagesdb1]:checked').val(),
        newcfsdb2: newcfsdb2,
        cfsdb2: $('input:radio[name=chauffagesdb2]:checked').val(),
        newcm: newcm,
        cfcm: $('input:radio[name=cumulus]:checked').val(),
        cfvol: $('input:checkbox[name=volet]:checked').val(),
        cfsto: $('input:checkbox[name=store]:checked').val()
    };
    $.each($(".escpl"), function(i, o){
        if ($(o).is(":checked")){
            vals["vl_" + $(o).attr("dindex") + "_" + i] = $(o).attr("dvalue");
        }
    });
    console.log(vals);
    $.post("/Home/DoActions", vals, function (data) {
        if (data.success) {
            $(".esys-waitstep").html("Réception des informations");
            $.post("/Home/WaitBox", function (data2) {
                if (data2.success) {
                    allowform();
                    $('.esys-zone').find('span').hide();
                    $('.esys-validinfo').removeClass('tovalidate');
                    //$("input[data-initial=true]").removeAttr("data-initial");
                    //$("input[checked=checked]").attr("data-initial", "true");
                    $('#volet').removeAttr('checked');
                    $('#store').removeAttr('checked');
                    refreshzones(data2, function(){$.colorbox.close();});
                }
                else {
                    $('.esys-wait').html(data2.message + "<br/><a href='#'>Réessayer</a>");
                    $('.esys-wait a').bind('click', function () {
                        $.colorbox.close();
                        window.location = "/";
                    });
                    $('.esys-wait').css('background', 'none');
                }
            });
        }
        else {
            if (data.redirect)
                window.location = "/";
            $('.esys-wait').html(data.message + "<br/><a href='#' id='esys-syncretry' style='display:inline-block'>Réessayer</a>&nbsp;&nbsp;<a href='#' id='esys-syncundo' style='display:inline-block'>Fermer</a>");
            $('#esys-syncretry').bind('click', function () {
                $('.esys-wait').html("Synchronisation avec Essensys<br/>Veuillez patienter<div class='esys-waitstep'></div>");
                $('.esys-wait').css('background-image', 'url(/Content/images/load.gif)');
                $('.esys-wait').css('background-repeat', 'no-repeat');
                $('.esys-wait').css('background-position', 'center bottom');
                $('body').css('scrollTop', $(document).height());
                doactions();
            });
            $('#esys-syncundo').bind('click', function () {
                window.location = "/Account/Logout";
            });
            $('.esys-wait').css('background', 'none');
        }
    });
};

function allowalarme() {
    $('.alarmeoption').removeClass('disabled');
    $('.alarmeoption input').removeAttr('disabled');
};
function disallowalarme(withcpt) {
    $('.alarmeoption').addClass('disabled');
    $("#question").val("");
    $('.alarmeoption input').attr('disabled', 'disabled');
    if (withcpt) {
        cptalarme = cptalarme * 2;
        disallowform();
        setTimeout(function () { allowform(); }, cptalarme);
    }
};
function unauthorizealarme(){
    disallowalarme(false);
    //$("#question").attr('disabled', 'disabled');
    //$(".alertmodifalarme").remove();
    $.colorbox({
        html: "<div class='esys-versiondesc'>Impossible d'activer le système d'alarme car l'écran de contrôle Essensys est actuellement utilisé. Vous devez attendre que celui-ci soit en veille avant de réessayer</div><br/><div style='text-align:center;font-weight:bold'><a href='#' class='esys-okmsg'>OK</a></div></div>",
        overlayClose: false,
        escKey: false,
        width:"300px",
        height:"200px"
    });
    $(".esys-okmsg").bind("click", function(){
        $.colorbox.close();
    });
    //$("#question").after("<div style='color:red' class='alertmodifalarme'>Modification alarme impossible car l'écran de contrôle est en cours d'utilisation</div>");
};
function disallowform() {
    $('body').css('opacity', '0.2');
    $('input').attr('disabled', 'disabled');
};
function allowform() {
    $('body').css('opacity', '1');
    $('input').removeAttr('disabled');
    disallowalarme(false);
}

function savephone(obj, cb){
    var id = $("#phone").attr('data-id');
    if (id == undefined)
        id = 0;
    $.post('/Phone/AddOrUpdatePhone', { id: id, phone: $("#phone").val(), sendmail:$("#sendmail").is(":checked")}, function(data){
        if (data.success){
            obj.val(data.phone.Phone);
            obj.attr('data-id', data.phone.Id);
            if (cb != null)
                cb.call();
        }
    });
};

function refreshzones(data, cb) {
    var state = data.state;
    if (!data.alarmeok)
        unauthorizealarme();
    var nocb = false;
    $.each(state, function (i, o) {
        if (o.Index.IndexKey == "938") {
            var value = o.Value;
            if (value == "1"){
                $('.storezone').addClass('disabled');
                $('.storezone input').attr('disabled', 'disabled');
            }
            else{
                $('.voletstorezone').addClass('disabled');
                $('.voletstorezone input').attr('disabled', 'disabled');
            }
        }
        
        if (o.Index.IndexKey == "349") {
            var value = o.Value;
            value = traitevaleurchauffage(value);
            $("#chauffagezj[data-initial=true]").removeAttr("data-initial");
            $("#chauffagezj[value='" + value + "']").attr('checked', 'checked');
            $("#chauffagezj[value='" + value + "']").attr('data-initial', 'true');
        }
        if (o.Index.IndexKey == "350") {
            var value = o.Value;
            value = traitevaleurchauffage(value);
            $("#chauffagezn[data-initial=true]").removeAttr("data-initial");
            $("#chauffagezn[value='" + value + "']").attr('checked', 'checked');
            $("#chauffagezn[value='" + value + "']").attr('data-initial', 'true');
        }
        if (o.Index.IndexKey == "351") {
            var value = o.Value;
            value = traitevaleurchauffage(value);
            $("#chauffagesdb1[data-initial=true]").removeAttr("data-initial");
            $("#chauffagesdb1[value='" + value + "']").attr('checked', 'checked');
            $("#chauffagesdb1[value='" + value + "']").attr('data-initial', 'true');
        }
        if (o.Index.IndexKey == "352") {
            var value = o.Value;
            value = traitevaleurchauffage(value);
            $("#chauffagesdb2[data-initial=true]").removeAttr("data-initial");
            $("#chauffagesdb2[value='" + value + "']").attr('checked', 'checked');
            $("#chauffagesdb2[value='" + value + "']").attr('data-initial', 'true');
        }
        if (o.Index.IndexKey == "363") {
            $("#arrosage[data-initial=true]").removeAttr("data-initial");
            $("#arrosage[value='" + o.Value + "']").attr('checked', 'checked');
            $("#arrosage[value='" + o.Value + "']").attr('data-initial', 'true');
        }
        if (o.Index.IndexKey == "353") {
            $("#cumulus[data-initial=true]").removeAttr("data-initial");
            $("#cumulus[value='" + o.Value + "']").attr('checked', 'checked');
            $("#cumulus[value='" + o.Value + "']").attr('data-initial', 'true');
        }
        if (o.Index.IndexKey == "920") {
            if (o.Value.substr(0, 1) == "0"){
                $("#alarme[value='off']").attr('checked', 'checked');
                $("#alarme[value='off']").attr('data-initial', 'true');
                $("#alarme[value='on']").removeAttr('data-initial');
                // Retrait off et demande on
                if (ar == "on"){
                    unauthorizealarme();
                    nocb = true;
                }
            }
            else{
                $("#alarme[value='on']").attr('checked', 'checked');
                $("#alarme[value='on']").attr('data-initial', 'true');
                $("#alarme[value='off']").removeAttr('data-initial');
                // Retrait on et demande off
                if (ar == "off"){
                    unauthorizealarme();
                    nocb = true;
                }
            }
        }
    });
    if (!nocb && cb != undefined){
        cb.call();
    }
};

function traitevaleurchauffage(value){
    value = Number(value);
    if (value >= 0 && value < 6)
        value = "1";
    if (value > 32 && value < 38)
        value = "32";
    if (value == 16)
        value = "16";
    return value;
};