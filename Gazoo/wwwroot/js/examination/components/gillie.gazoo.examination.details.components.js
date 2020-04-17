(function () {
    'use strict';

    angular
        .module('Company.gazoo.examination')
        .controller('examinationDetailsController', examinationDetailsController);

    angular
        .module('Company.gazoo.examination')
        .component('examinationDetails', {
            controller: 'examinationDetailsController',
            templateUrl: '/templates/examination/examinationDetails.html',
            bindings: {
                examId: "<"
            }
        });

    examinationDetailsController.$inject = ['$scope', '$ngBootbox', '$translate', '$q', 'promiseCommonService', 'examinationHttpService', 'alertService'];

    function examinationDetailsController($scope, $ngBootbox, $translate, $q, promiseCommonService, examinationHttpService, alertService) {
        var self = this;
        var questions = [];
        var answers = [];
        var audioFiles;
        var selectedAudioLength = 0;
        var examination = {};

        $scope.questionForm = {};
        $scope.examinationInputForm = {};
        $scope.examination;
        $scope.answerTypes = convertEnumToObject(AnswerType);
        $scope.temporaryAudioFile = {};
        $scope.answersToDisplay;
        $scope.questionsPaginated;
        $scope.questionAudioFiles;

        $scope.isQuestionEdit = false;

        $scope.selectedAnswer = {
            id: null,
            type: AnswerType.Script,
            name: ''
        };

        $scope.question = {
            answers: [],
            perfectReactionTime: 0
        };

        $scope.pagination = {
            currentPage: 1,
            itemsPerPage: 10,
            totalItems: 0,
            maxVisiblePages: 5
        };

        $scope.initialize = function () {
            getExamination(self.examId);
        };

        function getExamination(examId) {
            $scope.fetchExamination(examId);
            $scope.fetchAudioFiles();
            $scope.fetchQuestions(examId);
        }

        $scope.fetchExamination = function (examId) {
            $scope.fetchExaminationPromise = promiseCommonService.createPromise(
                examinationHttpService.getExamination,
                examId,
                "ERROR_DURING_FETCHING_EXAMINATION",
                function (res) {
                    examination = res.data;
                    examination.creationTime = moment.utc(examination.creationTime).local().format("HH:mm DD.MM.YYYY");
                    examination.modificationTime = moment.utc(examination.modificationTime).local().format("HH:mm DD.MM.YYYY");
                    $scope.examination = angular.copy(examination);
                    getPredefinedAnswerSetName();
                    $scope.fetchAnswers();
                });
        };

        $scope.fetchQuestions = function (examId) {
            $scope.fetchQuestionsPromise = promiseCommonService.createPromise(
                examinationHttpService.getExaminationQuestions,
                examId,
                "ERROR_DURING_FETCHING_QUESTIONS",
                function (res) {
                    questions = res.data;
                    $scope.pagination.totalItems = questions.length;
                    $scope.setPage();
                    $scope.resetQuestionForm();
                });
        };

        function getPredefinedAnswerSetName() {
            promiseCommonService.createPromise(
                examinationHttpService.getPredefinedAnswerSetName,
                $scope.examination.predefinedAnswerSetId,
                "ERROR_DURING_DETTING_PREDEFINED_ANSWER_SETS",
                function (response) {
                    if (response.data.indexOf('_LABEL') !== -1) {
                        $scope.examination.answerSetName = $translate.instant(response.data);
                        return;
                    }

                    $scope.examination.answerSetName = response.data;
                }
            );
        }
       
        $scope.getAudioDurationInSeconds = function (id) {
            promiseCommonService.createPromise(
                examinationHttpService.getAudioDurationInSeconds,
                id,
                "ERROR_DURING_FETCHING_AUDIO_DURATION",
                function (res) {
                    $scope.question.questionAudioFile = res;
                });
        };

        $scope.downloadAudioFile = function (id) {
            var linkToAudio = examinationHttpService.getAudioFileLink(id);
            createtWaveSurferPlayer($scope.temporaryAudioFile);
            subscribeWavePlayerEvents($scope.temporaryAudioFile);
            $scope.temporaryAudioFile.wavePlayer.load(linkToAudio);
        };

        $scope.fetchAnswers = function () {
            $scope.fetchAnswersPromise = promiseCommonService.createPromise(
                examinationHttpService.getAllAnswers,
                $scope.examination.predefinedAnswerSetId,
                "ERROR_DURING_FETCHING_ANSWERS",
                function (res) {
                    answers = res.data;
                    $scope.answersToDisplay = answers.scripts;
                });
        };

        $scope.fetchAudioFiles = function () {
            $scope.fetchAudioFilesPromise = promiseCommonService.createPromise(
                examinationHttpService.getAllExaminationAudiosPromise,
                null,
                "ERROR_DURING_FETCHING_EXAMINATION_AUDIOS",
                function (res) {
                    audioFiles = res.data;
                    differenceAudioFiles();
                });
        };

        $scope.confirmDeleteQuestion = function (questionId) {
            $ngBootbox.confirm($translate.instant("DELETE_QUESTION_CONFIRMATION"))
                .then(function () {
                    deleteQuestion(questionId);
                });
        };

        $scope.setPage = function () {
            var skip = ($scope.pagination.currentPage - 1) * $scope.pagination.itemsPerPage;
            $scope.questionsPaginated = questions.slice(skip, skip + $scope.pagination.itemsPerPage);
        };

        $scope.updateExamData = function () {
            var request = {
                id: $scope.examination.id,
                name: $scope.examination.name,
                reactionTime: $scope.examination.reactionTime
            };

            $scope.updateExaminationPromise = promiseCommonService.createPromise(
                examinationHttpService.updateExamination,
                request,
                "ERROR_DURING_UPDATING_EXAMINATION",
                function () {
                    if ($scope.examination.reactionTime !== examination.reactionTime) {
                        fixQuestions();
                    }
                    $scope.fetchExamination($scope.examination.id);
                });
        };

        $scope.changeRandomState = function () {
            $scope.updateExaminationRandomStatePromise = promiseCommonService.createPromise(
                examinationHttpService.changeRandomState,
                $scope.examination.id,
                "ERROR_DURING_UPDATING_EXAMINATION",
                () => { $scope.fetchExamination($scope.examination.id);});
        };

        $scope.resetExamData = function () {
            $scope.examination.name = examination.name;
            $scope.examination.reactionTime = examination.reactionTime;
            setFormElementValidity($scope.examinationInputForm, 'examinationTitle', true);
        };

        $scope.isExaminationChanged = () => {
            return $scope.examination.name !== examination.name || $scope.examination.reactionTime !== examination.reactionTime;
        };

        $scope.addAnswerToQuestion = function () {
            if (!$scope.selectedAnswer.id)
                return;

            var value = angular.copy($scope.answersToDisplay.find(answer => answer.id === $scope.selectedAnswer.id));
            value.type = $scope.selectedAnswer.type;

            $scope.selectedAnswer.name = "";
            $scope.selectedAnswer.id = null;

            var answerWithOpenedQueue = $scope.question.answers.find(answer => answer.openedQueue === true);

            if (answerWithOpenedQueue) {
                answerWithOpenedQueue.nextAnswers.push(value);
                return;
            }

            $scope.question.answers.unshift(value);
            resetQuestionFormElements();
            $scope.onChangeAudioFileName();
            $scope.onChangeAnswerType();
        };

        $scope.openQueue = function (answer) {
            closeOpenedQueue();
            if (!answer.nextAnswers) {
                answer.nextAnswers = [];
            }
            answer.openedQueue = true;
        };

        $scope.closeQueue = function (answer) {
            deleteQueueIfEmpty(answer);
            answer.openedQueue = false;
        };

        $scope.deleteAnswerFromQueue = function (answer, queuedAnswer) {
            answer.nextAnswers = answer.nextAnswers.filter(answer => answer.id !== queuedAnswer.id);
            deleteQueueIfEmpty(answer);
        };

        $scope.deleteAnswerFromQuestion = function (answerIndex) {
            $scope.question.answers.splice(answerIndex, 1)[0];
        };

        $scope.saveQuestion = function () {
            if (!$scope.questionForm.$invalid) {
                if ($scope.isQuestionEdit)
                    editQuestion();
                else
                    addQuestion();
            }
        };

        $scope.editQuestion = function (question) {
            $scope.isQuestionEdit = true;
            
            $scope.question = angular.copy(question);
            $scope.question.questionAudioFile = audioFiles.find(item => item.id === question.audioFileId);
            $scope.temporaryAudioFile.loadingFile = true;
            $scope.downloadAudioFile($scope.question.audioFileId);
        };

        $scope.resetQuestionForm = function () {
            if ($scope.isQuestionEdit)
                $scope.fetchQuestions(examination.id);
            $scope.isQuestionEdit = false;

            $scope.question = {
                answers: [],
                perfectReactionTime: 0,
                questionAudioFile: {}
            };

            $scope.selectedAnswer = {
                id: null,
                type: AnswerType.Script,
                name: ''
            };

            resetQuestionFormElements();
            $scope.onChangeAudioFileName();
            $scope.onChangeAnswerType();
        };

        $scope.getClassForDisplay = (answerType) => {
            switch (answerType) {
                case AnswerType.Script: return 'label-success';
                case AnswerType.Objection: return 'label-warning';
                case AnswerType.QuickAnswer: return 'label-primary';
            }
        };

        $scope.getAnswerTypeLabel = (answerType) => $scope.answerTypes.find(type => type.value === answerType).label;

        $scope.onExaminationNameChanged = function () {
            if (!$scope.examination.name) {
                setFormElementValidity($scope.examinationInputForm, 'examinationTitle', false);
                return;
            }

            promiseCommonService.createPromise(
                examinationHttpService.checkExaminationNameExists,
                $scope.examination.name,
                "ALERT_ERROR_TITLE",
                function (responce) {
                    if (responce.data && $scope.examination.name !== examination.name) {
                        setFormElementValidity($scope.examinationInputForm, 'examinationTitle', false);
                        return;
                    }

                    setFormElementValidity($scope.examinationInputForm, 'examinationTitle', true);
                });
        };

        $scope.onChangeAnswerType = () => {
            $scope.selectedAnswer.name = '';
            resetFormElement($scope.questionForm, 'answer');

            switch ($scope.selectedAnswer.type) {
                case AnswerType.Script: $scope.answersToDisplay = angular.copy(answers.scripts); return;
                case AnswerType.Objection: $scope.answersToDisplay = angular.copy(answers.objections); return;
                case AnswerType.QuickAnswer: $scope.answersToDisplay = angular.copy(answers.quickAnswers); return;
            }
        };

        $scope.onChangeAnswerName = function () {
            if (!$scope.selectedAnswer.name) {
                resetFormElement($scope.questionForm, 'answer');
                return;
            }

            var answer = $scope.answersToDisplay.find(answer => answer.name === $scope.selectedAnswer.name);

            if (!answer || checkIfAnswerUsing(answer.id)) {
                $scope.selectedAnswer.id = null;
                setFormElementValidity($scope.questionForm, 'answer', false);
                return;
            }

            $scope.selectedAnswer.id = answer.id;
            setFormElementValidity($scope.questionForm, 'answer', true);
        };

        $scope.onChangeReactionTime = function () {
            if ($scope.question.perfectReactionTime === 0 && !$scope.question.answers.length) {
                resetFormElement($scope.questionForm, 'perfectTime');
                return;
            }

            if ($scope.question.perfectReactionTime <= 0 || $scope.question.perfectReactionTime > selectedAudioLength + $scope.examination.reactionTime) {
                setFormElementValidity($scope.questionForm, 'perfectTime', false);
                return;
            }

            setFormElementValidity($scope.questionForm, 'perfectTime', true);
        };

        $scope.onChangeAudioFileName = function () {
            if ($scope.isQuestionEdit)
                return;

            if (!$scope.question.questionAudioFile.name && !$scope.question.answers.length) {
                selectedAudioLength = 0;
                $scope.question.perfectReactionTime = 0;
                resetFormElement($scope.questionForm, 'questionAudioFile');
                $scope.onChangeReactionTime();
                return;
            }

            var audioFile = $scope.questionAudioFiles.find(audioFile => audioFile.name === $scope.question.questionAudioFile.name);

            if (!audioFile) {
                selectedAudioLength = 0;
                $scope.question.perfectReactionTime = 0;
                setFormElementValidity($scope.questionForm, 'questionAudioFile', false);
                $scope.onChangeReactionTime();
                return;
            }

            $scope.temporaryAudioFile.loadingFile = true;
            $scope.question.questionAudioFile = Object.assign({}, audioFile);
            $scope.downloadAudioFile($scope.question.questionAudioFile.id);
            setFormElementValidity($scope.questionForm, 'questionAudioFile', true);
        };

        function checkIfAnswerUsing(answerId) {
            var answerWithQueue = $scope.question.answers.find(answer => answer.openedQueue === true);

            if (answerWithQueue) {
                return answerWithQueue.id === answerId || answerWithQueue.nextAnswers.some(answer => answer.id === answerId);
            }

            return $scope.question.answers.some(answer => answer.id === answerId);
        }

        function resetFormElement(form, elementName) {
            setFormElementValidity(form, elementName, true);
            form[elementName].$setPristine();
            form[elementName].$setUntouched();
        }

        function setFormElementValidity(form, elementName, isValid) {
            form[elementName].$setValidity("validName", isValid);
        }

        function resetQuestionFormElements() {
            if (!$scope.question.questionAudioFile.name)
                resetFormElement($scope.questionForm, 'questionAudioFile');
            if (!$scope.question.answers.length)
                resetFormElement($scope.questionForm, 'perfectTime');

            resetFormElement($scope.questionForm, 'answer');
        }

        function closeOpenedQueue() {
            var openedAnswerQueue = $scope.question.answers.find(answer => answer.openedQueue === true);

            if (openedAnswerQueue) {
                deleteQueueIfEmpty(openedAnswerQueue);
                openedAnswerQueue.openedQueue = false;
            }
        }

        function deleteQueueIfEmpty(answer) {
            if (answer.nextAnswers && !answer.nextAnswers.length)
                delete answer.nextAnswers;
        }

        function fixQuestions() {
            questions.forEach(function (entry) {
                getAudioDuration(entry);
            });
        }

        function checkIfNeedToRepair(entry, duration) {
            if (entry.perfectReactionTime > duration + $scope.examination.reactionTime) {
                entry.examination = $scope.examination;
                entry.perfectReactionTime = duration + $scope.examination.reactionTime;
                promiseCommonService.createPromise(
                    examinationHttpService.updateQuestion,
                    entry,
                    "ERROR_DURING_UPDATING_ANSWER",
                    function (res) {
                        $scope.fetchQuestions($scope.examination.id);
                    });
            }
        }

        function getAudioDuration(entry) {
            var linkToAudio = examinationHttpService.getAudioFileLink(entry.audioFileId);
            var tempAudio = {};
            createtWaveSurferPlayer(tempAudio);
            tempAudio.wavePlayer.on('ready', function () {
                $scope.$apply(function () {
                    checkIfNeedToRepair(entry, tempAudio.wavePlayer.getDuration());
                });
            });
            tempAudio.wavePlayer.load(linkToAudio);
        }

        function createtWaveSurferPlayer(file) {
            var elementId = "#containerForWavesurfer";

            file.wavePlayer = WaveSurfer.create({
                container: elementId,
                waveColor: 'violet',
                progressColor: 'purple',
                height: 40
            });
        }

        function subscribeWavePlayerEvents(file) {
            file.wavePlayer.on('ready', function () {
                $scope.$apply(function () {
                    file.loadingFile = false;
                    selectedAudioLength = file.wavePlayer.getDuration();
                    if (!$scope.isQuestionEdit && !$scope.question.perfectReactionTime) {
                        $scope.question.perfectReactionTime = selectedAudioLength;
                        $scope.onChangeReactionTime();
                    }
                });
            });

            file.wavePlayer.on('error', function (message) {
                alertService.showError("ERROR_ON_FETCHING_AUDIO_FILE");
                $scope.$apply(function () {
                    file.loadingFile = false;
                });
            });
        }

        function deleteQuestion(questionId) {
            $scope.deleteQuestionPromise = promiseCommonService.createPromise(
                examinationHttpService.deleteQuestion,
                questionId,
                "ERROR_DURING_DELETING_QUESTION",
                function (res) {
                    $scope.fetchQuestions($scope.examination.id);
                    differenceAudioFiles();
                    $scope.fetchExamination($scope.examination.id);
                });
        }

        function addQuestion() {
            closeOpenedAnswersQueue();
            var request = {
                questionAudioFileId: $scope.question.questionAudioFile.id,
                examinationId: $scope.examination.id,
                perfectReactionTime: $scope.question.perfectReactionTime,
                answers: $scope.question.answers
            };

            $scope.addQuestionPromise = promiseCommonService.createPromise(
                examinationHttpService.addQuestion,
                request,
                "ERROR_DURING_ADDING_ANSWER",
                function (res) {
                    $scope.fetchQuestions($scope.examination.id);
                    differenceAudioFiles();
                    $scope.resetQuestionForm();
                    $scope.fetchExamination($scope.examination.id);
                });
        }

        function editQuestion() {
            closeOpenedAnswersQueue();
            var request = {
                questionId: $scope.question.id,
                questionAudioFileId: $scope.question.audioFileId,
                examinationId: $scope.examination.id,
                perfectReactionTime: $scope.question.perfectReactionTime,
                answers: $scope.question.answers
            };

            $scope.updateQuestionPromise = promiseCommonService.createPromise(
                examinationHttpService.updateQuestion,
                request,
                "ERROR_DURING_UPDATING_ANSWER",
                function (res) {
                    $scope.fetchQuestions($scope.examination.id);
                    $scope.resetQuestionForm();
                    $scope.fetchExamination($scope.examination.id);
                });
        }

        function closeOpenedAnswersQueue() {
            $scope.question.answers.forEach(function (entry) {
                $scope.closeQueue(entry);
            });
        }

        function differenceAudioFiles() {
            $q.all([$scope.fetchQuestionsPromise]).then(function (res1) {
                $scope.questionAudioFiles = difference(audioFiles, questions.map(question => question.audioFileId));
            });
        }

        function difference(arr1, arr2) {
            return arr1.filter(function (arr2Item) {
                return arr2.indexOf(arr2Item.id) < 0;
            });
        }

        function convertEnumToObject(data) {
            return Object.keys(data).map(key => {
                return {
                    value: data[key],
                    label: 'EXAMINATION_ANSWER_TYPE_' + key.replace(/([a-z])([A-Z])/g, '$1_$2').toUpperCase() + 'S_LABEL'
                };
            });
        }

        $scope.getSaveQuestionLabel = () => {
            if (!$scope.isQuestionEdit)
                return 'ADD_LABEL';

            return 'SAVE_BUTTON';
        };
    }
})();