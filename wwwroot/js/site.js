console.log("site.js loaded");

// ================= LISTING BUTTON =================
$(document).on("click", ".btn-enquiry", function () {
    $("#listing_institute_id").val($(this).data("id"));
    $("#listing_college").val($(this).data("college"));
    $("#listing_page_url").val(window.location.href);
    $("#enquiryModal").modal("show");
});

// ================= DETAILS BUTTON =================
$(document).on("click", ".btn-phone-details", function () {
    $("#details_institute_id").val($(this).data("id"));
    $("#phoneModal").modal("show");
});

// ================= GET RECAPTCHA TOKEN =================
function getRecaptchaToken(action) {
    return new Promise(function (resolve) {
        if (typeof grecaptcha === "undefined") {
            resolve(""); // reCAPTCHA not loaded, skip
            return;
        }
        grecaptcha.ready(function () {
            grecaptcha.execute(window.recaptchaSiteKey, { action: action })
                .then(function (token) { resolve(token); });
        });
    });
}

// ================= LISTING SUBMIT =================
$(document).on("submit", "#enquiryFormListing", function (e) {
    e.preventDefault();

    var isCollegeListPage = window.location.pathname.indexOf("-colleges-in-") !== -1;
    var listingSubmitUrl = isCollegeListPage ? "/College/Enquiry/Submit" : "/Enquiry/Submit";

    var formData = {
        Name:      $("#listing_name").val(),
        Email:     $("#listing_email").val(),
        Course:    $("#listing_course").val(),
        Phone:     $("#listing_phone").val(),
        Message:   $("#listing_message").val(),
        College:   $("#listing_college").val(),
        InstituteId: parseInt($("#listing_institute_id").val()) || 0,
        PageUrl:   $("#listing_page_url").val(),
        QueryType: "Enquiry",
        Honeypot:  $("#listing_honeypot").val() // Must be empty
    };

    getRecaptchaToken("enquiry").then(function (token) {
        formData.RecaptchaToken = token;

        $.ajax({
            url: listingSubmitUrl,
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(formData),
            success: function () {
                $("#enquiryModal").modal("hide");
                setTimeout(function () {
                    $("#msg_content").html(
                        "Thank you <b>" + formData.Name +
                        "</b>, your enquiry has been sent to <b>" +
                        formData.College + "</b>"
                    );
                    $("#msg_popup").modal("show");
                }, 400);
            },
            error: function (xhr) {
                if (xhr.status === 429) {
                    alert("Too many submissions. Please try again in a few minutes.");
                } else {
                    alert("Something went wrong. Please try again.");
                }
            }
        });
    });
});

// ================= DETAILS SUBMIT =================
$(document).on("submit", "#enquiryFormDetails", function (e) {
    e.preventDefault();

    var isCollegePage = window.location.pathname.indexOf("/college/") === 0;
    var detailsSubmitUrl = isCollegePage ? "/College/Enquiry/Submit" : "/Enquiry/Submit";

    var formData = {
        Name:      $("#details_name").val(),
        Email:     $("#details_email").val(),
        Course:    $("#details_course").val(),
        Phone:     $("#details_phone").val(),
        Message:   $("#details_message").val(),
        College:   $(".school-title").first().text().trim(),
        InstituteId: parseInt($("#details_institute_id").val()) || 0,
        PageUrl:   window.location.href,
        QueryType: "PhoneReveal",
        Honeypot:  $("#details_honeypot").val() // Must be empty
    };

    getRecaptchaToken("phone_reveal").then(function (token) {
        formData.RecaptchaToken = token;

        $.ajax({
            url: detailsSubmitUrl,
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(formData),
            success: function (res) {
                $("#phoneModal").modal("hide");
                if (res.phone) {
                    $("#phoneText").html(
                        '<a href="tel:' + res.phone + '">' + res.phone + '</a>'
                    );
                    setTimeout(function () {
                        var instituteName = $(".school-title").first().text().trim() || "";
                        $("#msg_content").html(
                            "Thank you <b>" + formData.Name + "</b>!<br><br>" +
                            "<b>" + instituteName + "</b><br>" +
                            "Phone: <b><a href='tel:" + res.phone + "'>" + res.phone + "</a></b>"
                        );
                        $("#msg_popup").modal("show");
                    }, 400);
                }
            },
            error: function (xhr) {
                if (xhr.status === 429) {
                    alert("Too many submissions. Please try again in a few minutes.");
                } else {
                    alert("Something went wrong. Please try again.");
                }
            }
        });
    });
});

// ================= READ MORE =================
$(document).on("click", ".readMoreLink", function (e) {
    e.preventDefault();
    var parent = $(this).closest(".top-content");
    parent.find(".moreText").slideToggle();
    $(this).text(function (i, text) {
        return text === "Read More" ? "Read Less" : "Read More";
    });
});

$(document).ready(function () {
    $(".top-content .moreText").hide();
});

// ================= SEARCH =================
$("#searchBox").autocomplete({
    source: function (request, response) {
        $.ajax({
            url: "/Search/AutoComplete",
            data: { term: request.term },
            success: function (data) { response(data); }
        });
    },
    minLength: 2,
    select: function (event, ui) {
        window.location.href = ui.item.url;
    }
});