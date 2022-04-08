document.addEventListener('DOMContentLoaded', function () {
    var inputs = document.querySelectorAll(".input_elem");
    InitInputs(inputs);
    var form = document.querySelector("form");
    var submit_button = document.getElementById("submit-button");
    form.addEventListener('input', function () {
        var validate = $(form).validate();
        if (validate.checkForm()) {
            submit_button.removeAttribute('disabled');
        }
        else {
            submit_button.setAttribute('disabled', 'true');
        }
        validate.submitted = {};
    });
    //$("#Login_Form").submit(function (e) {
    //    var error_summary = $("#error_summary");
    //    var formdata = new FormData($(this).get(0));
    //    $.ajax({
    //        url: "/Account/Login",
    //        type: 'POST',
    //        data: formdata,
    //        processData: false,
    //        contentType: false,
    //        success: function (result) {
    //            console.log(document.querySelector("html"));
    //            document.querySelector("html").innerHTML = result;
    //            //if (result.error != null) {
    //            //    console.log(result);
    //            //    console.log(error_summary);
    //            //    error_summary.css({ "display": "flex" });
    //            //    error_summary.text(result.error);
    //            //}
    //            //else if (result.success == "true") {
    //            //    location.reload();
    //            //}
    //        }
    //    });
    //    e.preventDefault();
    //})
});
function Input(InputElement, ClearButton, EyeButton, Loader) {
    this.input = InputElement;
    this.clearButton = ClearButton;
    this.eyeButton = EyeButton;
    this.loader = Loader;
    //ClearButton
    this.createClearButton = function () {
        if (this.clearButton) {
            const that = this;
            this.input.addEventListener("focus", function () {
                if (this.value) {
                    that.clearButton.style.display = "flex";
                }
            });
            this.input.addEventListener("blur", function () {
                that.clearButton.style.display = "";
            });
            this.input.addEventListener("input", function () {
                if (this.value.length >= 1 && !that.clearButton.style.display) {
                    that.clearButton.style.display = "flex";
                }
                if (this.value.length < 1 && that.clearButton.style.display == "flex") {
                    that.clearButton.style.display = "";
                }
            })
            this.clearButton.addEventListener("mousedown", function (e) {
                e.preventDefault();
                that.input.value = "";
                this.style.display = "";
            });
        }
    };
    //Loader
    this.createLoader = function () {
        if (this.loader) {
            const that = this;
            let timeout = null;
            this.input.addEventListener("input", function () {
                that.loader.style.display = "block";
                clearTimeout(timeout);
                // Make a new timeout set to go off in 1000ms (1 second)
                timeout = setTimeout(function () {
                    that.loader.style.display = "none";
                }, 500);
            })
        }
    };
    //EyeButton
    this.createEyeButton = function () {
        if (this.eyeButton) {
            const that = this;
            this.eyeButton.addEventListener("click", function (e) {
                e.preventDefault();
                let svg = this.querySelector("use");
                if (that.input.getAttribute('type') == 'password') {
                    svg.setAttribute('xlink:href', '/Icons/outline/20/eye-closed.svg#eye_closed_icon');
                    that.input.setAttribute('type', 'text');
                } else {
                    svg.setAttribute('xlink:href', '/Icons/outline/20/eye-open.svg#eye_open_icon');
                    that.input.setAttribute('type', 'password');
                }
                return false;
            })
        }
    }
    //CreateAll
    this.createAllEvents = function () {
        this.createClearButton();
        this.createEyeButton();
        this.createLoader();
    }
}
function InitInputs(inputs) {
    inputs.forEach(input => {
        let currentInput = new Input(
            InputElement = input,
            ClearButton = input.parentElement.querySelector("#clear_button"),
            EyeButton = input.parentElement.querySelector("#eye_button"),
            Loader = input.parentElement.parentElement.querySelector(".loader-wrapper")
        );
        currentInput.createAllEvents();
    });
}