(function () {
    'use strict';

    angular
        .module('Company.gazoo.app')
        .service('examinationHttpService', examinationHttpService);

    examinationHttpService.$inject = ['httpRequestManager'];

    function examinationHttpService(httpRequestManager) {

        var url = "/api/Examination/";
        var jsonContentType = "application/json";

        this.getFilteredExaminations = function (data) {
            return httpRequestManager.createHttpPostRequest(url + "GetFilteredExaminations", data, jsonContentType);
        };

        this.deleteExamination = function (data) {
            return httpRequestManager.createHttpPostRequest(url + "DeleteExamination", data, jsonContentType);
        };

        this.addExamination = function (data) {
            return httpRequestManager.createHttpPostRequest(url + "AddExamination", data, jsonContentType);
        };

        this.checkExaminationNameExists = function (examTitle) {
            return httpRequestManager.createHttpGetRequest(url + "CheckExaminationNameExists/" + examTitle);
        };

        this.getExamination = function (data) {
            return httpRequestManager.createHttpPostRequest(url + "GetExaminationInfo", data, jsonContentType);
        };

        this.getExaminationQuestions = function (data) {
            return httpRequestManager.createHttpPostRequest(url + "GetExaminationQuestions", data, jsonContentType);
        };

        this.updateExamination = function (data) {
            return httpRequestManager.createHttpPostRequest(url + "UpdateExamination", data, jsonContentType);
        };

        this.changeRandomState = function (data) {
            return httpRequestManager.createHttpPostRequest(url + "ChangeRandomState", data, jsonContentType);
        };

        this.getAnswers = function (data) {
            return httpRequestManager.createHttpGetRequest(url + "GetAnswers/" + data);
        };

        this.getAllAnswers = function (data) {
            return httpRequestManager.createHttpGetRequest(url + "GetAllAnswers/" + data);
        };

        this.deleteQuestion = function (data) {
            return httpRequestManager.createHttpPostRequest(url + "DeleteQuestion", data, jsonContentType);
        };

        this.checkAnswerExists = function (data) {
            return httpRequestManager.createHttpPostRequest(url + "CheckAnswerExists", data, jsonContentType);
        };

        this.addAnswer = function (data) {
            return httpRequestManager.createHttpPostRequest(url + "AddAnswer", data, jsonContentType);
        };

        this.addQuestion = function (data) {
            return httpRequestManager.createHttpPostRequest(url + "AddQuestion", data, jsonContentType);
        };

        this.updateQuestion = function (data) {
            return httpRequestManager.createHttpPostRequest(url + "UpdateQuestion", data, jsonContentType);
        };

        this.checkAnswerIsUsed = function (data) {
            return httpRequestManager.createHttpGetRequest(url + "CheckAnswerIsUsed/" + data);
        };

        this.checkAnswerSetIsUsed = function (data) {
            return httpRequestManager.createHttpGetRequest(url + "CheckAnswerSetIsUsed/" + data);
        };

        this.getExaminationsForReportsPromise = function (data) {
            return httpRequestManager.createHttpPostRequest(url + "GetExaminationsForReports", data, jsonContentType);
        };

        this.getAgentsForReportsPromise = function (data) {
            return httpRequestManager.createHttpPostRequest(url + "GetAgentsForReports?tenant=" + data.source, data, jsonContentType);
        };

        this.getExaminationReportsPromise = function (data) {
            return httpRequestManager.createHttpPostRequest(url + "GetExaminationReports?tenant=" + data.source, data, jsonContentType);
        };

        this.deleteAnswer = function (data) {
            return httpRequestManager.createHttpPostRequest(url + "DeleteAnswer", data, jsonContentType);
        };

        this.deleteAnswerSet = function (data) {
            return httpRequestManager.createHttpPostRequest(url + "DeleteAnswerSet", data, jsonContentType);
        };

        this.getExaminationReport = function (reportId) {
            return httpRequestManager.createHttpGetRequest(url + "GetExaminationReport/"+reportId);
        };

        this.getInstances = function (data) {
            return httpRequestManager.createHttpGetRequest(url + "GetAvailableInstancesToUser", jsonContentType);
        };

        this.getExaminationStatistics = function (data) {
            return httpRequestManager.createHttpPostRequest(url + "GetExaminationStatistics", data, jsonContentType);
        };

        this.getPredefinedAnswerSets = () => {
            return httpRequestManager.createHttpGetRequest(url + "GetPredefinedAnswerSets", jsonContentType);
        };

        this.checkAnswerSetNameExists = (data) => {
            return httpRequestManager.createHttpGetRequest(url + "CheckAnswerSetNameExists/" + data);
        };

        this.addAnswerSet = (data) => {
            return httpRequestManager.createHttpPostRequest(url + "AddAnswerSet", data, jsonContentType);
        };

        this.updateAnswerSet = (data) => {
            return httpRequestManager.createHttpPostRequest(url + "UpdateAnswerSet", data, jsonContentType);
        };

        this.getPredefinedAnswerSetName = (data) => {
            return httpRequestManager.createHttpGetRequest(url + "GetPredefinedAnswerSetName/" + data);
        };

        this.getAllExaminationAudiosPromise = function () {
            return httpRequestManager.createHttpGetRequest(url + "GetAllExaminationAudios", null, jsonContentType);
        };

        this.deleteSelecedAudioPromise = function (request) {
            return httpRequestManager.createHttpPostRequest(url + "DeleteSelecedAudio", request, jsonContentType);
        };

        this.checkIfFileExistOnServer = function (request) {
            return httpRequestManager.createHttpGetRequest(url + "CheckIfFileExist/" + request);
        };

        this.updateAudioFileInfoPromise = function (request) {
            return httpRequestManager.createHttpPostRequest(url + "UpdateAudioFileInfo", request, jsonContentType);
        };

        this.uploadAudioFilesPromise = function (request) {
            return httpRequestManager.createHttpPostRequest(url + "SaveExaminationAudio", request);
        };

        this.getAudioFileLink = function (id) {
            return url + "GetAudioFile/" + id;
        };
    }
})(); 