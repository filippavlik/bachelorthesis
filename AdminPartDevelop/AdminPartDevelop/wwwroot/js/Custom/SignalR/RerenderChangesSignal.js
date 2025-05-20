const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubForReendering")
    .build();

connection.start()
    .then(() => {
        console.log("Connection started. Waiting for server messages...");
    })
    .catch(err => {
        console.error(err.toString());
    });
// Set up client-side event handlers for the methods your server will call
// Set up client-side event handlers for the methods your server will call
connection.on("AcceptChangeMatchAdd", function (matchId, refereeId, refereeName, role, user, timestampChange) {

    let matchDelegationPane;
    if (role === 0) {
        matchDelegationPane = "referee_delegation_pane_" + matchId;
    }
    else if (role === 1) {
        matchDelegationPane = "ar1_delegation_pane_" + matchId;
    }
    else if (role === 2) {
        matchDelegationPane = "ar2_delegation_pane_" + matchId;
    }
    const $paneDelegationWrapper = $("#" + matchDelegationPane);
    var $parentDiv = $paneDelegationWrapper.closest('.position-relative');


    const $button = $(`
        <button class="btn btn-secondary referee-button" data-id="${refereeId}"
            style="position: absolute;height:25px;width:80px;padding:0;display:flex;align-items: center;justify-content: center;flex-direction: row;margin-left: 30px;margin-top: 5px;">
            <strong>${refereeName}</strong>
        </button>
    `);

    $parentDiv.append($button);
    const whoChanged = "last_changed_by_" + matchId;
    const $whoChangedDiv = $("#" + whoChanged);
    $whoChangedDiv.text(user);

    const whenChanged = "last_changed_" + matchId;
    const $whenChangedDiv = $("#" + whenChanged);
    const date = new Date(timestampChange);

    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0'); // Months are 0-indexed
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    const seconds = String(date.getSeconds()).padStart(2, '0');

    const formatted = `${day}/${month} ${hours}:${minutes}:${seconds}`;

    $whenChangedDiv.text(formatted);

});

connection.on("AcceptChangeMatchRemove", function (matchId, refereeId, user, timestampChange) {
    const matchPane = "match_pane_" + matchId;
    const $paneWrapper = $("#" + matchPane);

    $paneWrapper.find('button[data-id="' + refereeId + '"]').remove();
    const whoChanged = "last_changed_by_" + matchId;
    const $whoChangedDiv = $("#" + whoChanged);
    $whoChangedDiv.text(user);

    const whenChanged = "last_changed_" + matchId;
    const $whenChangedDiv = $("#" + whenChanged);

    const date = new Date(timestampChange);

    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0'); // Months are 0-indexed
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    const seconds = String(date.getSeconds()).padStart(2, '0');

    const formatted = `${day}/${month} ${hours}:${minutes}:${seconds}`;

    $whenChangedDiv.text(formatted);

});

connection.on("AcceptChangeReferee", function (data) {
    console.log("som tu na zaciatku");
    if (!data) {
        console.error("Received undefined or null data");
        return;
    }

    if (!data.refereeId) {
        console.error("Expected property 'refereeId' is missing:", data);
        return;
    }
    const buttonId = data.refereeId + "_referee-offer-button";
    const $buttonWrapper = $("#" + buttonId);

    if ($buttonWrapper.length) {
        $buttonWrapper.empty();

        // Add left side rectangles based on updated data
        if (data.refereeData.isFreeSaturdayMorning) {
            $buttonWrapper.append('<div class="rectangle rect-left-top" style="background-color: yellow;"></div>');
        }
        if (data.refereeData.isFreeSaturdayAfternoon) {
            $buttonWrapper.append('<div class="rectangle rect-left-bottom" style="background-color: darkblue;"></div>');
        }

        if (data.refereeData.isFreeSundayMorning) {
            $buttonWrapper.append('<div class="rectangle rect-right-top" style="background-color: darkred;"></div>');
        }
        if (data.refereeData.isFreeSundayAfternoon) {
            $buttonWrapper.append('<div class="rectangle rect-right-bottom" style="background-color: forestgreen;"></div>');
        }
        const buttonHtml = `
                  <button id="${data.refereeId}_referee-offer-pure-button" class="btn btn-secondary referee-button" data-id="${data.refereeId}">
                      <strong>${data.refereeData.referee.name.substring(0, 1)}. ${data.refereeData.referee.surname}</strong>
                  </button>
                `;
        $buttonWrapper.append(buttonHtml);
        $.ajax({
            url: 'Admin/Referee/UploadRefreshedMatch',
            type: 'POST',
            data: { matchId: data.refereeData.matchId },
            success: function (updatedMatches) {
            },
            error: function (xhr, status, error) {
                console.error("Failed to refresh match", error);
            }
        });
    }
    else {
        console.log("Button with ID " + buttonId + " not found");
    }
});
connection.on("AcceptMatchLockUpdate", function (matchId, lockStatus, user, timestampChange) {

    const matchPane = "match_pane_" + matchId;
    const $paneWrapper = $("#" + matchPane);

    if (!lockStatus) {
        $paneWrapper.find("input, select, button, textarea").not(".lock-button").prop("disabled", false);
        $paneWrapper.removeClass("disabled-pane");
    } else {
        $paneWrapper.find("input, select, button, textarea").not(".lock-button").prop("disabled", true);
        $paneWrapper.addClass("disabled-pane");
    }

    const whoChanged = "last_changed_by_" + matchId;
    const $whoChangedDiv = $("#" + whoChanged);
    $whoChangedDiv.text(user);

    const whenChanged = "last_changed_" + matchId;
    const $whenChangedDiv = $("#" + whenChanged);
    const date = new Date(timestampChange);

    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0'); // Months are 0-indexed
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    const seconds = String(date.getSeconds()).padStart(2, '0');

    const formatted = `${day}/${month} ${hours}:${minutes}:${seconds}`;

    $whenChangedDiv.text(formatted);
});


