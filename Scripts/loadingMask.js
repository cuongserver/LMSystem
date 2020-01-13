function activateLoadingMask() {
    $("#loading-mask").addClass("loading-mask-active");
};

function deactivateLoadingMask() {
    setTimeout(function () {
        $("#loading-mask").removeClass("loading-mask-active");
    }, 1000);
};