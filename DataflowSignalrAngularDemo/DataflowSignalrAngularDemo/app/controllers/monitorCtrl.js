// the monitor service is "constructor injected" into the controller

function monitorCtrl($scope, monitorSvc, $rootScope) {
    $scope.monitors = new Array();
    $scope.Processor = new Array();
    $scope.FileOrderEntities = new Array();

    $scope.sendmonitor = function () {
        monitorSvc.sendRequest();
    };
    
    var addProcessor = function (data) {
        $scope.Processor.push(data);
    };

    var addFileOrderEntity = function (data) {
        $scope.FileOrderEntities.push(data);
    };

    monitorSvc.initialize();
    
    $scope.$parent.$on("LoadBalance", function (e, data) {
        $scope.$apply(function () {
            addProcessor(data);
        });
    });


    $scope.$parent.$on("TransformFileToFileOrderEntity", function (e, data) {
        $scope.$apply(function () {
            addFileOrderEntity(data);
        });
    });
}