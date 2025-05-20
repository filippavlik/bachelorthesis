$(function () {
    let counterColor = 0;
    const colors = [
        'violet', 'purple', 'maroon', 'crimson',
        'orchid', 'salmon', 'red', 'ivory',
        'peach', 'gray', 'yellow', 'coral', 'chocolate',
        'teal', 'aqua', 'lime', 'magenta', 'turquoise',
        'gold', 'white', 'beige', 'brown', 'lavender',
        'orange', 'emerald', 'indigo', 'blue', 'plum'
    ];

    $("div[id^='match_pane_']").each(function () {
        // Get the current div
        let matchPane = $(this);

        const prematchDiv = matchPane.find("#match_div_prematch input").val().trim();
        const postmatchDiv = matchPane.find("#match_div_postmatch input").val().trim();

        //if this is true we can start propagation of coloring
        if (prematchDiv === "" && postmatchDiv !== "") {
            matchPane.css("background-color", colors[counterColor]);
            propagateColoring(postmatchDiv, colors[counterColor]);
            counterColor = (counterColor + 1) % colors.length; // Cycle through colors
        }       
    });

    function propagateColoring(idOfNextMatch, groupColor) {
        const nextMatchPane = $("#match_pane_" + idOfNextMatch);
        if (nextMatchPane.length > 0) {
            nextMatchPane.css("background-color", groupColor);
            const nextPostmatch = nextMatchPane.find("#match_div_postmatch input").val().trim();
            if (nextPostmatch !== "") {
                propagateColoring(nextPostmatch, groupColor);
            }
        }
    }
});
