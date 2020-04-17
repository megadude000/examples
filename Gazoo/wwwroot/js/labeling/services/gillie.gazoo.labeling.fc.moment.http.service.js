(function () {
    'use strict';

    angular
        .module('Company.gazoo.app')
        .service('fcMomentHttpService', fcMomentHttpService);

    fcMomentHttpService.$inject = ['httpRequestManager'];

    function fcMomentHttpService(httpRequestManager) {
        var url = "/api/FCMoments/";
        var jsonContentType = "application/json";

        this.saveAudioWithData = function (request) {
            return httpRequestManager.createHttpPostRequest(url + "SaveAudioWithData", request);
        };

        this.releaseAudio = (id) => {
            return httpRequestManager.createHttpPostRequest(url + "ReleaseAudio", id, jsonContentType);
        };

        this.getAudioForNextAmsPrediction = () => {
            return httpRequestManager.createHttpGetRequest(url + "GetAudioForProcessing", null, jsonContentType);
        };

        this.getAudioForNextAmsVerification = () => {
            return httpRequestManager.createHttpGetRequest(url + "GetAudioForVerification", null, jsonContentType);
        };

        this.saveMomentPredictionResult = (data) => {
            return httpRequestManager.createHttpPostRequest(url + "SaveFCMomentResult", data, jsonContentType);
        };

        this.saveMomentVerificationResult = (data) => {
            return httpRequestManager.createHttpPostRequest(url + "SaveFCMomentVerificationResult", data, jsonContentType);
        };

        this.getAudioFileLink = function (id) {
            return url + "GetAudioFile/" + id;
        };

        this.getImportFileNames = (id) => {
            return httpRequestManager.createHttpGetRequest(url + "GetImportFileNames/" + id);
        };
    }
})(); 