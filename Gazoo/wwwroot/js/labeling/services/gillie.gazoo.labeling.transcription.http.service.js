(function () {
    'use strict';

    angular
        .module('Company.gazoo.app')
        .service('transacriptionHttpService', transacriptionHttpService);

    transacriptionHttpService.$inject = ['httpRequestManager'];

    function transacriptionHttpService(httpRequestManager) {
        var url = "/api/Transcription/";
        var jsonContentType = "application/json";

        this.releaseAudio = function (data) {
            return httpRequestManager.createHttpPostRequest(url + "ReleaseAudio", data, jsonContentType);
        };

        this.saveAudioTrancription = function (data) {
            return httpRequestManager.createHttpPostRequest(url + "SaveAudioTrancription", data, jsonContentType);
        };

        this.saveAudioVerification = function (data) {
            return httpRequestManager.createHttpPostRequest(url + "SaveAudioVerification", data, jsonContentType);
        };

        this.getAudioForTranscription = function () {
            return httpRequestManager.createHttpGetRequest(url + "GetAudioForTranscription", null, jsonContentType);
        };

        this.getAudioForVerification = function () {
            return httpRequestManager.createHttpGetRequest(url + "GetAudioForVerification", null, jsonContentType);
        };

        this.getLabelGroups = function () {
            return httpRequestManager.createHttpGetRequest(url + "GetLabelGroups", null, jsonContentType);
        };

        this.getLabels = function (data) {
            return httpRequestManager.createHttpGetRequest(url + "GetLabelGroups/" + data);
        };

        this.checkLabelGroupNameExists = function (data) {
            return httpRequestManager.createHttpGetRequest(url + "CheckLabelGroupNameExists/" + data);
        };

        this.checkLabelNameExists = function (data) {
            return httpRequestManager.createHttpGetRequest(url + "CheckLabelNameExists/" + data);
        };

        this.getLabels = function (data) {
            return httpRequestManager.createHttpGetRequest(url + "GetLabels/" + data);
        };

        this.addLabelGroup = function (data) {
            return httpRequestManager.createHttpPostRequest(url + "AddLabelGroup", data, jsonContentType);
        };

        this.deleteLabelGroup = function (labelGroupId) {
            return httpRequestManager.createHttpPostRequest(url + "DeleteLabelGroup", labelGroupId, jsonContentType);
        };

        this.deleteLabel = function (labelId) {
            return httpRequestManager.createHttpPostRequest(url + "DeleteLabel", labelId, jsonContentType);
        };

        this.getNewImportNumber = function (data) {
            return httpRequestManager.createHttpPostRequest(url + "GetNewImportNumber", data, jsonContentType);
        };

        this.addLabel = function (data) {
            return httpRequestManager.createHttpPostRequest(url + "AddLabel", data, jsonContentType);
        };

        this.updateLabelGroup = function (data) {
            return httpRequestManager.createHttpPostRequest(url + "UpdateLabelGroup", data, jsonContentType);
        };

        this.updateLabel = function (data) {
            return httpRequestManager.createHttpPostRequest(url + "UpdateLabel", data, jsonContentType);
        };

        this.getAgentsForReportsPromise = function (data) {
            return httpRequestManager.createHttpPostRequest(url + "GetAgentsForReports", data, jsonContentType);
        };

        this.getGeneralStatistics = function (data) {
            return httpRequestManager.createHttpPostRequest(url + "GetGeneralStatistics", data, jsonContentType);
        };

        this.getTranscriptionStatistics = function (data) {
            return httpRequestManager.createHttpPostRequest(url + "GetTranscriptionStatistics", data, jsonContentType);
        };

        this.getMomentsPredictionStatistics = function (data) {
            return httpRequestManager.createHttpPostRequest(url + "GetMomentsPredictionStatistics", data, jsonContentType);
        };

        this.getAudioFileLink = function (id) {
            return url + "GetAudioFile/" + id;
        };

        this.getImportStatistics = function (data) {
            return httpRequestManager.createHttpPostRequest(url + "GetImportStatistics", data, jsonContentType);
        };

        this.updateImport = (data) => {
            return httpRequestManager.createHttpPostRequest(url + "UpdateImport", data, jsonContentType);
        };

        this.getImportFileNames = (id) => {
            return httpRequestManager.createHttpGetRequest(url + "GetImportFileNames/" + id);
        };

        this.resetRecordsVerification = (data) => {
            return httpRequestManager.createHttpGetRequest(url + "ResetRecordsVerification/" + data);
        };

        this.getCampaignInstancePairing = () => {
            return httpRequestManager.createHttpGetRequest(url + "GetCampaignInstancePairing", null, jsonContentType);
        };
    }
})(); 