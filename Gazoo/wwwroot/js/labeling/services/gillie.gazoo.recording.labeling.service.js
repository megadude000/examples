(function () {
    'use strict';

    angular
        .module('Company.gazoo.app')
        .service('recordingLabelingService', recordingLabelingService);

    recordingLabelingService.$inject = ['httpRequestManager'];

    function recordingLabelingService(httpRequestManager) {
        var url = "/api/RecordingLabeling/";
        var jsonContentType = "application/json";

        this.getClientUtterances = (id) => {
            return httpRequestManager.createHttpGetRequest(url + "GetClientUtterances/" + id);
        };

        this.getAudioMessageLogUpdates = (id) => {
            return httpRequestManager.createHttpGetRequest(url + "GetAudioMessageLogUpdates/" + id);
        };

        this.updateCallLabelingAsync = (data) => {
            return httpRequestManager.createHttpPostRequest(url + "UpdateRecordingLabeling", data, jsonContentType);
        };
    }
})();