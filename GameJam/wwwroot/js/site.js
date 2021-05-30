function Delete(deleteObject, deleteType) {
    if (deleteObject === null || deleteObject === undefined ||
        deleteType === null || deleteType === undefined) return;

    const deleteForm = $(`#${deleteType}`);
    if (deleteForm === undefined || deleteForm === null) return;

    const url = deleteForm.attr("action");
    const type = deleteForm.attr("method");
    const token = document.querySelector('input[name="__RequestVerificationToken"]').getAttribute("value");

    if (token === undefined || token === null) return;

    const value = deleteObject.getAttribute("value");
    const gameId = deleteObject.getAttribute("gameId");
    if (value === null ||
        gameId === null ||
        gameId === undefined ||
        value === undefined) return;

    const formData = new FormData();
    formData.append("Value", value);
    formData.append("GameId", gameId);

    $.ajax({
        url: url,
        type: type,
        headers: {
            "RequestVerificationToken": token
        },
        contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
        data: Object.fromEntries(formData),
        success: function (response) {
            window.location = response;
        },
        failure: function (response) {
            window.location = response;
        },
        error: function (response) {
            window.location = response;
        }
    });
}

function ShowDelete(element) {

    const exists = document.getElementById("Delete") != undefined;

    if (exists) return;

    const value = element.getAttribute("value");
    const gameId = element.getAttribute("gameId");
    if (value === null ||
        gameId === null ||
        gameId === undefined ||
        value === undefined) return;

    const deleteElement = document.createElement("div");
    deleteElement.className += "spill-item-delete";
    deleteElement.id = "Delete";

    const deleteIcon = document.createElement("i");
    deleteIcon.className += "fas fa-trash spill-item-delete-icon";
    deleteIcon.title = "Click to remove video.";

    deleteIcon.setAttribute("value", value);
    deleteIcon.setAttribute("gameId", gameId);

    deleteElement.appendChild(deleteIcon);
    element.appendChild(deleteElement);

    deleteIcon.addEventListener("click", () => Delete(deleteIcon, "DeleteVideo"));

    deleteElement.addEventListener("mouseleave", () => HideDelete());
}

function HideDelete() {
    const divToRemove = document.getElementById("Delete");
    if (divToRemove === null || divToRemove === undefined) return;

    divToRemove.remove();
}

$(document).ready(function () {
    $(".rating").map(function (index, element) {
        return $(element).on("rating.change",
            function (event, value, caption) {
                const gameId = element.getAttribute("gameId");
                if (gameId === null || gameId === undefined) {
                    alert("Null");
                    return;
                }
                const ratingInput = $(`#rating-${gameId}`);
                if (ratingInput === undefined || ratingInput === null) return;
                ratingInput.val(value);
            });
    }).get();
});