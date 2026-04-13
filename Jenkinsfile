// ═══════════════════════════════════════════════════════════════════════════
// Airline Transaction Monitor — Declarative Jenkins Pipeline
// ═══════════════════════════════════════════════════════════════════════════
//
// This pipeline builds, tests, analyzes, and packages the entire stack:
//   - 4 .NET 10 services (Gateway, Ingestion, Analytics, Simulator)
//   - 1 Angular 21 dashboard
//
// Prerequisites on the Jenkins agent:
//   - .NET SDK 10.0
//   - Node.js 24.x + npm
//   - Docker + Docker Compose
//   - dotnet-sonarscanner global tool
//   - SonarQube server accessible at SONAR_HOST_URL
//
// Quality gates:
//   - All .NET tests must pass (43 xUnit tests)
//   - All Angular tests must pass (14 Vitest tests)
//   - SonarQube quality gate must pass
//   - Docker images must build successfully

pipeline {
    agent any

    environment {
        DOTNET_CLI_TELEMETRY_OPTOUT = '1'
        DOTNET_NOLOGO                = '1'
        SONAR_HOST_URL              = credentials('sonar-host-url')     // http://sonarqube:9000
        SONAR_TOKEN                 = credentials('sonar-token')        // Project token
        SONAR_PROJECT_KEY           = 'airline-transaction-monitor'
        DOCKER_REGISTRY             = 'ghcr.io/peelmicro'               // GitHub Container Registry
    }

    options {
        timeout(time: 30, unit: 'MINUTES')
        disableConcurrentBuilds()
        buildDiscarder(logRotator(numToKeepStr: '10'))
    }

    stages {
        // ─── Stage 1: Restore & Build ──────────────────────────────────
        stage('Restore & Build') {
            steps {
                echo '🔧 Restoring NuGet packages and building .NET solution...'
                sh 'dotnet restore'
                sh 'dotnet build --configuration Release --no-restore'
            }
        }

        // ─── Stage 2: .NET Tests ───────────────────────────────────────
        stage('.NET Tests') {
            steps {
                echo '🧪 Running xUnit tests with code coverage...'
                sh '''
                    dotnet test --configuration Release --no-build \
                        --logger "trx;LogFileName=results.trx" \
                        --collect:"XPlat Code Coverage" \
                        --results-directory TestResults
                '''
            }
            post {
                always {
                    // Publish test results to Jenkins
                    mstest testResultsFile: '**/TestResults/**/*.trx'
                    // Publish code coverage
                    publishHTML(target: [
                        allowMissing: true,
                        alwaysLinkToLastBuild: true,
                        keepAll: true,
                        reportDir: 'TestResults',
                        reportFiles: '**/coverage.cobertura.xml',
                        reportName: '.NET Code Coverage'
                    ])
                }
            }
        }

        // ─── Stage 3: SonarQube Analysis ───────────────────────────────
        stage('SonarQube Analysis') {
            steps {
                echo '📊 Running SonarQube static analysis...'
                sh '''
                    dotnet sonarscanner begin \
                        /k:"${SONAR_PROJECT_KEY}" \
                        /d:sonar.host.url="${SONAR_HOST_URL}" \
                        /d:sonar.token="${SONAR_TOKEN}" \
                        /d:sonar.cs.opencover.reportsPaths="**/TestResults/**/coverage.opencover.xml" \
                        /d:sonar.cs.vstest.reportsPaths="**/TestResults/**/*.trx" \
                        /d:sonar.exclusions="**/bin/**,**/obj/**,**/node_modules/**,**/dist/**,**/.angular/**,**/Migrations/**"

                    dotnet build --configuration Release --no-restore

                    dotnet sonarscanner end /d:sonar.token="${SONAR_TOKEN}"
                '''
            }
        }

        // ─── Stage 4: SonarQube Quality Gate ───────────────────────────
        stage('Quality Gate') {
            steps {
                echo '🚦 Waiting for SonarQube quality gate result...'
                timeout(time: 5, unit: 'MINUTES') {
                    waitForQualityGate abortPipeline: true
                }
            }
        }

        // ─── Stage 5: Angular Build & Test ─────────────────────────────
        stage('Angular Build & Test') {
            steps {
                echo '🌐 Installing Angular dependencies, running tests, and building...'
                dir('apps/web') {
                    sh 'npm ci'
                    sh 'npx ng test --watch=false --code-coverage'
                    sh 'npx ng build --configuration production'
                }
            }
            post {
                always {
                    publishHTML(target: [
                        allowMissing: true,
                        alwaysLinkToLastBuild: true,
                        keepAll: true,
                        reportDir: 'apps/web/coverage',
                        reportFiles: 'index.html',
                        reportName: 'Angular Code Coverage'
                    ])
                }
            }
        }

        // ─── Stage 6: Docker Image Builds ──────────────────────────────
        stage('Docker Images') {
            parallel {
                stage('Gateway Image') {
                    steps {
                        sh 'docker build -t ${DOCKER_REGISTRY}/atm-gateway:${BUILD_NUMBER} -f apps/gateway/Dockerfile .'
                    }
                }
                stage('Ingestion Image') {
                    steps {
                        sh 'docker build -t ${DOCKER_REGISTRY}/atm-ingestion:${BUILD_NUMBER} -f apps/ingestion/Dockerfile .'
                    }
                }
                stage('Analytics Image') {
                    steps {
                        sh 'docker build -t ${DOCKER_REGISTRY}/atm-analytics:${BUILD_NUMBER} -f apps/analytics/Dockerfile .'
                    }
                }
                stage('Simulator Image') {
                    steps {
                        sh 'docker build -t ${DOCKER_REGISTRY}/atm-simulator:${BUILD_NUMBER} -f apps/simulator/Dockerfile .'
                    }
                }
                stage('Web Image') {
                    steps {
                        sh 'docker build -t ${DOCKER_REGISTRY}/atm-web:${BUILD_NUMBER} -f apps/web/Dockerfile .'
                    }
                }
            }
        }

        // ─── Stage 7: Push to Registry (commented out for assessment) ──
        // Uncomment when deploying to a real environment.
        // stage('Push to Registry') {
        //     steps {
        //         echo '📦 Pushing Docker images to registry...'
        //         withCredentials([usernamePassword(
        //             credentialsId: 'docker-registry-creds',
        //             usernameVariable: 'DOCKER_USER',
        //             passwordVariable: 'DOCKER_PASS'
        //         )]) {
        //             sh 'echo $DOCKER_PASS | docker login ${DOCKER_REGISTRY} -u $DOCKER_USER --password-stdin'
        //             sh 'docker push ${DOCKER_REGISTRY}/atm-gateway:${BUILD_NUMBER}'
        //             sh 'docker push ${DOCKER_REGISTRY}/atm-ingestion:${BUILD_NUMBER}'
        //             sh 'docker push ${DOCKER_REGISTRY}/atm-analytics:${BUILD_NUMBER}'
        //             sh 'docker push ${DOCKER_REGISTRY}/atm-simulator:${BUILD_NUMBER}'
        //             sh 'docker push ${DOCKER_REGISTRY}/atm-web:${BUILD_NUMBER}'
        //         }
        //     }
        // }

        // ─── Stage 8: Deploy (mock for assessment) ─────────────────────
        stage('Deploy (Mock)') {
            steps {
                echo '🚀 Deploy stage — in production this would:'
                echo '   1. Push images to container registry'
                echo '   2. Update Kubernetes manifests or Helm values'
                echo '   3. Apply rolling update via kubectl or ArgoCD'
                echo '   4. Run smoke tests against the deployed environment'
                echo ''
                echo "Build ${BUILD_NUMBER} ready for deployment."
            }
        }
    }

    post {
        success {
            echo '✅ Pipeline completed successfully!'
        }
        failure {
            echo '❌ Pipeline failed. Check the logs above for details.'
        }
        always {
            // Clean up workspace
            cleanWs()
        }
    }
}
