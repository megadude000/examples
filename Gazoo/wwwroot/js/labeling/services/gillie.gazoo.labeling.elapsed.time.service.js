(function () {
    'use strict';

    angular
        .module('Company.gazoo.app')
        .service('elapsedTimeService', elapsedTimeService);

    elapsedTimeService.$inject = ['$state'];

    function elapsedTimeService($state) {
        var totalSpentTime = 0;
        var startTime = null;
        var timeoutTimer = null;
        var delay = 10000;

        var eventTypes =
        {
            composition: [
                "composition",
                "compositionstart",
                "compositionend"
            ],

            contextmenu: [
                "contextmenu"
            ],

            drag: [
                "dragenter",
                "dragover",
                "dragexit",
                "dragdrop",
                "draggesture"
            ],

            focus: [
                "focus",
                "blur"
            ],

            form: [
                "submit",
                "reset",
                "change",
                "select",
                "input"
            ],

            key: [
                "keydown",
                "keyup",
                "keypress"
            ],

            load: [
                "load",
                "beforeunload",
                "unload",
                "abort",
                "error"
            ],

            mouse: [
                "mousedown",
                "mouseup",
                "click",
                "dblclick",
                //"mouseover",
                //"mouseout",
                //"mousemove"
            ],

            paint: [
                "paint",
                "resize",
                "scroll"
            ],

            scroll: [
                "overflow",
                "underflow",
                "overflowchanged"
            ],

            text: [
                "text"
            ],

            ui: [
                "DOMActivate",
                "DOMFocusIn",
                "DOMFocusOut"
            ],

            clipboard: [
                "cut",
                "copy",
                "paste"
            ]
        };

        this.startTimer = () => {
            totalSpentTime = 0;
            if (sessionStorage.labelingSpendedTime)
                totalSpentTime = parseInt(sessionStorage.labelingSpendedTime);

            attachAllListeners();
            resetTimer();
        };

        this.stopTimer = () => {


            totalSpentTime += getElapsedTimeSinceStartTime();
            var result = angular.copy(totalSpentTime);
            detachAllListeners();

            return result;
        };

        function resetTimer() {
            if ($state.current.name !== "transcription" && $state.current.name !== "nextAmsPrediction")
                return;

            if (!startTime) {
                startTime = new Date().getTime();
            }

            if (timeoutTimer)
                clearTimeout(timeoutTimer);

            timeoutTimer = setTimeout(countSpentTime, delay);
        }

        function countSpentTime() {
            totalSpentTime += getElapsedTimeSinceStartTime();
        }

        function attachAllListeners() {
            for (var family in eventTypes) {
                var types = eventTypes[family];
                for (var i = 0; i < types.length; ++i)
                    window.addEventListener(types[i], resetTimer, false);
            }

            addRefreshEventListener();
        }

        function detachAllListeners() {
            for (var family in eventTypes) {
                var types = eventTypes[family];
                for (var i = 0; i < types.length; ++i)
                    window.removeEventListener(types[i], resetTimer, false);
            }

            removeRefreshEventListener();
        }

        function getElapsedTimeSinceStartTime() {
            clearTimeout(timeoutTimer);
            var spentTime = 0; 

            if (startTime) {
                spentTime = new Date().getTime() - startTime;
                startTime = null;
            }

            return spentTime;
        }

        function addRefreshEventListener() {
            window.addEventListener('beforeunload', (event) => {
                countSpentTime();
                sessionStorage.labelingSpendedTime = totalSpentTime;
            });
        }

        function removeRefreshEventListener() {
            window.removeEventListener('beforeunload', (event) => {
                countSpentTime();
                sessionStorage.labelingSpendedTime = totalSpentTime;
            });

            delete sessionStorage.labelingSpendedTime;
        }
    }
})();