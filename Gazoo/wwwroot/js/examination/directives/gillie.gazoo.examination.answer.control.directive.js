(function () {
    'use strict';

    angular
        .module('Company.gazoo.examination')
        .component('answerControl', {
            controller: 'answerControlController',
            templateUrl: 'templates/examination/answerControl.html',
            bindings: {
                type: "<",
                setId: "<"
            }
        });

    angular
        .module('Company.gazoo.examination')
        .controller('answerControlController', answerControlController);

    answerControlController.$inject = ['$scope', 'promiseCommonService', 'alertService', '$translate', 'examinationHttpService', '$ngBootbox'];

    function answerControlController($scope, promiseCommonService, alertService, $translate, examinationHttpService, $ngBootbox) {
        var self = this;
        $scope.answers = [];
        $scope.answerType = AnswerType;
        $scope.paginatedAnswers = [];
        $scope.answerValue = null;

        $scope.pagination = {
            currentPage: 1,
            itemsPerPage: 25,
            totalItems: 0,
            maxVisiblePages: 7
        };

        $scope.initialize = () => {
            $scope.fetchAnswers();
        };

        $scope.setPage = function () {
            var skip = ($scope.pagination.currentPage - 1) * $scope.pagination.itemsPerPage;
            $scope.paginatedAnswers = $scope.answers.slice(skip, skip + $scope.pagination.itemsPerPage);
        };

        $scope.getInputPlaceholderLabel = () => {
            switch (self.type) {
                case AnswerType.Script: return $translate.instant('ADD_EXAMINATION_ANSWER_TYPE_SCRIPTS_LABEL');
                case AnswerType.Objection: return $translate.instant('ADD_EXAMINATION_ANSWER_TYPE_OBJECTIONS_LABEL');
                case AnswerType.QuickAnswer: return $translate.instant('ADD_EXAMINATION_ANSWER_TYPE_QUICK_ANSWERS_LABEL');
            }
        };

        $scope.getHeaderLabel = () => {
            switch (self.type) {
                case AnswerType.Script: return $translate.instant('EXAMINATION_ANSWER_TYPE_SCRIPTS_LABEL');
                case AnswerType.Objection: return $translate.instant('EXAMINATION_ANSWER_TYPE_OBJECTIONS_LABEL');
                case AnswerType.QuickAnswer: return $translate.instant('EXAMINATION_ANSWER_TYPE_QUICK_ANSWERS_LABEL');
            }
        };

        $scope.answerValueChanged = (value) => {
            if (!value) {
                $scope.answerForm.answerValue.$setValidity("validName", true);
                return;
            }

            $scope.checkAnswerExistspromise = promiseCommonService.createPromise(
                examinationHttpService.checkAnswerExists,
                addSetIdToRequestIfDefined({
                    name: value,
                    type: self.type
                }),
                "ALERT_ERROR_TITLE",
                function (response) {
                    if (response.data) {
                        $scope.answerForm.answerValue.$setValidity("validName", false);
                        return;
                    }

                    $scope.answerForm.answerValue.$setValidity("validName", true);
                });
        };

        $scope.addAnswer = (value) => {
            $scope.addAnswerPromise = promiseCommonService.createPromise(
                examinationHttpService.addAnswer,
                addSetIdToRequestIfDefined({
                    name: value,
                    type: self.type
                }),
                "ERROR_DURING_ADDING_ANSWER_VALUE",
                function (response) {
                    if (!response) {
                        alertService.showError("ERROR_DURING_ADDING_ANSWER_VALUE");
                        return;
                    }

                    $scope.answerValue = null;
                    $scope.fetchAnswers();
                });
        };

        $scope.fetchAnswers = function () {
            $scope.fetchAnswersPromise = promiseCommonService.createPromise(
                examinationHttpService.getAnswers,
                formFetchRequest(),
                "ERROR_DURING_FETCHING_ANSWER_VALUES",
                function (response) {
                    if (!response) {
                        alertService.showError("ERROR_DURING_FETCHING_ANSWER_VALUES");
                        return;
                    }

                    setRecievedAnswers(response.data);
                });
        };

        function setRecievedAnswers(data) {
            $scope.answers = data;
            $scope.pagination.totalItems = data.length;
            $scope.setPage();
        }

        $scope.confirmDeleteAnswerValue = function (answerId) {
            $ngBootbox.confirm($translate.instant("ANSWER_DELETE_CONFIRMATION"))
                .then(function () {
                    checkAnswerIsUsed(answerId);
                });
        };

        function checkAnswerIsUsed(answerId) {
            $scope.checkIsAnswerUsingPromise = promiseCommonService.createPromise(
                examinationHttpService.checkAnswerIsUsed,
                answerId,
                "ALERT_ERROR_TITLE",
                function (response) {
                    if (response.data) {
                        alertService.showWarning("CONFIGURATION_IS_USED_ERROR");
                        return;
                    }

                    deleteAnswer(answerId);
                });
        }

        function deleteAnswer(answerId) {
            $scope.deleteAnswerPromise = promiseCommonService.createPromise(
                examinationHttpService.deleteAnswer,
                answerId,
                "ERROR_DURING_DELETING_ANSWER_VALUE",
                function (response) {
                    $scope.fetchAnswers();
                });
        }

        function formFetchRequest() {
            if (!self.setId)
                return self.type;

            return self.type + '/' + self.setId;
        }

        function addSetIdToRequestIfDefined(request) {
            if (!self.setId)
                return request;

            request.setId = self.setId;

            return request;
        }
    }
})();