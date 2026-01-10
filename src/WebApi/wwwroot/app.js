// Propgic Property Analysis Dashboard - JavaScript

const API_BASE_URL = '/api/PropertyAnalyser';

// Current analysis data
let currentAnalysis = null;
let currentPropertyData = null;

// DOM Elements
const propertyInput = document.getElementById('propertyInput');
const analyzeBtn = document.getElementById('analyzeBtn');
const analyzeBtnText = document.getElementById('analyzeBtnText');
const analyzeBtnSpinner = document.getElementById('analyzeBtnSpinner');
const loadingState = document.getElementById('loadingState');
const emptyState = document.getElementById('emptyState');
const resultsContainer = document.getElementById('resultsContainer');

// Initialize
document.addEventListener('DOMContentLoaded', () => {
    loadAnalyses();

    // Enter key to analyze
    propertyInput.addEventListener('keypress', (e) => {
        if (e.key === 'Enter') {
            analyzeProperty();
        }
    });

    // Analyze button click
    analyzeBtn.addEventListener('click', analyzeProperty);
});

// Determine if input is URL or address
function isUrl(input) {
    try {
        new URL(input);
        return true;
    } catch {
        return input.includes('domain.com.au') ||
               input.includes('realestate.com.au') ||
               input.includes('property.com.au');
    }
}

// Analyze property
async function analyzeProperty() {
    const input = propertyInput.value.trim();
    if (!input) {
        alert('Please enter a property address or URL');
        return;
    }

    setLoading(true);

    try {
        let response;
        const isUrlInput = isUrl(input);

        if (isUrlInput) {
            // Create analysis by URL
            response = await fetch(`${API_BASE_URL}/by-url`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    propertyUrl: input,
                    analyserType: 'PropertyAnchor'
                })
            });
        } else {
            // Create analysis by address
            response = await fetch(API_BASE_URL, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    propertyAddress: input,
                    analyserType: 'PropertyAnchor'
                })
            });
        }

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const analysis = await response.json();

        // Run the analysis
        const runResponse = await fetch(`${API_BASE_URL}/${analysis.id}/run`, {
            method: 'POST'
        });

        if (!runResponse.ok) {
            throw new Error(`Failed to run analysis: ${runResponse.status}`);
        }

        const completedAnalysis = await runResponse.json();
        displayAnalysis(completedAnalysis);
        loadAnalyses(); // Refresh the list

    } catch (error) {
        console.error('Error analyzing property:', error);
        alert('Error analyzing property: ' + error.message);
    } finally {
        setLoading(false);
    }
}

// Load all analyses
async function loadAnalyses() {
    try {
        const response = await fetch(API_BASE_URL);
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const analyses = await response.json();
        displayRecentAnalyses(analyses);

        // Check if we have any analyses
        if (analyses.length === 0) {
            // No analyses at all - show empty state
            showEmptyState();
            return;
        }

        // If none is currently displayed, try to show the latest completed one
        if (!currentAnalysis) {
            const latestCompleted = analyses.find(a => a.status === 'Completed');
            if (latestCompleted) {
                displayAnalysis(latestCompleted);
            } else {
                // No completed analyses - show empty state
                showEmptyState();
            }
        }

    } catch (error) {
        console.error('Error loading analyses:', error);
        // On error, show empty state
        showEmptyState();
    }
}

// Show empty state
function showEmptyState() {
    currentAnalysis = null;
    emptyState.classList.remove('hidden');
    resultsContainer.classList.add('hidden');
    loadingState.classList.add('hidden');
}

// Display analysis results
function displayAnalysis(analysis) {
    currentAnalysis = analysis;
    emptyState.classList.add('hidden');
    resultsContainer.classList.remove('hidden');

    // Parse analysis result JSON if available
    let resultData = null;
    if (analysis.analysisResult) {
        try {
            resultData = JSON.parse(analysis.analysisResult);
        } catch {
            resultData = null;
        }
    }

    // Update summary section
    document.getElementById('propertyAddressDisplay').textContent = analysis.propertyAddress;
    document.getElementById('analysisStatus').textContent = analysis.status;

    // Update score and recommendation
    const score = analysis.analysisScore || 0;
    document.getElementById('analysisScoreDisplay').textContent = score.toFixed(1) + '/100';
    document.getElementById('confidenceScore').textContent = score.toFixed(0) + '/100';

    // Determine recommendation based on score
    const recommendationText = document.getElementById('recommendationText');
    if (score >= 75) {
        recommendationText.textContent = 'Strong Buy';
        recommendationText.className = 'text-success text-3xl font-black leading-tight';
    } else if (score >= 60) {
        recommendationText.textContent = 'Consider';
        recommendationText.className = 'text-primary text-3xl font-black leading-tight';
    } else if (score >= 40) {
        recommendationText.textContent = 'Neutral';
        recommendationText.className = 'text-gray-600 text-3xl font-black leading-tight';
    } else {
        recommendationText.textContent = 'Caution';
        recommendationText.className = 'text-danger text-3xl font-black leading-tight';
    }

    // Update summary text
    document.getElementById('summaryText').textContent = analysis.remarks || 'Analysis completed. See detailed results below.';

    // Update metrics from result data
    if (resultData) {
        updateMetricsFromData(resultData);
        updateStrengthsAndRisks(resultData);
    } else {
        // Clear metrics
        document.getElementById('propertyImage').classList.add('hidden');
        document.getElementById('propertyImagePlaceholder').classList.remove('hidden');
        document.getElementById('rentalYieldDisplay').textContent = '--%';
        document.getElementById('capitalGrowthDisplay').textContent = '--%';
        document.getElementById('riskRatingDisplay').textContent = '--';
        document.getElementById('locationDisplay').textContent = '--';
        document.getElementById('propertyTypeDisplay').textContent = '--';
        document.getElementById('landOwnershipDisplay').textContent = '--';
        document.getElementById('vacancyRateDisplay').textContent = '--%';
        document.getElementById('localDemandDisplay').textContent = '-- Demand';
        document.getElementById('propertyAgeDisplay').textContent = '-- years';
        document.getElementById('maintenanceLevelDisplay').textContent = 'Maintenance: --';
        document.getElementById('schoolZoneDisplay').textContent = '--';
        document.getElementById('distanceToCbdDisplay').textContent = '-- km to CBD';
    }

    // Display raw analysis result
    document.getElementById('analysisResultText').textContent = analysis.analysisResult || 'No detailed results available.';
}

// Update metrics from property data
function updateMetricsFromData(data) {
    // Update property image
    const propertyImage = document.getElementById('propertyImage');
    const propertyImagePlaceholder = document.getElementById('propertyImagePlaceholder');

    if (data.imageUrl) {
        propertyImage.src = data.imageUrl;
        propertyImage.classList.remove('hidden');
        propertyImagePlaceholder.classList.add('hidden');
    } else {
        propertyImage.classList.add('hidden');
        propertyImagePlaceholder.classList.remove('hidden');
    }

    // Quick metrics
    document.getElementById('rentalYieldDisplay').textContent =
        data.rentalYieldPercentage ? data.rentalYieldPercentage.toFixed(1) + '%' : '--%';
    document.getElementById('capitalGrowthDisplay').textContent =
        data.capitalGrowthPercentage ? data.capitalGrowthPercentage.toFixed(1) + '%' : '--%';
    document.getElementById('riskRatingDisplay').textContent = data.riskRating || '--';
    // Display suburb if available, otherwise fall back to location category
    document.getElementById('locationDisplay').textContent = data.suburb || data.locationCategory || '--';

    // Property details
    document.getElementById('propertyTypeDisplay').textContent = data.propertyType || '--';
    document.getElementById('landOwnershipDisplay').textContent = data.landOwnership || '--';

    // Vacancy
    document.getElementById('vacancyRateDisplay').textContent =
        data.vacancyRatePercentage ? data.vacancyRatePercentage.toFixed(1) + '%' : '--%';
    const demandDisplay = document.getElementById('localDemandDisplay');
    demandDisplay.textContent = (data.localDemand || '--') + ' Demand';
    if (data.localDemand === 'High') {
        demandDisplay.className = 'text-xs font-bold text-success flex items-center gap-1';
    } else if (data.localDemand === 'Low') {
        demandDisplay.className = 'text-xs font-bold text-danger flex items-center gap-1';
    } else {
        demandDisplay.className = 'text-xs font-bold text-gray-500 flex items-center gap-1';
    }

    // Property age
    document.getElementById('propertyAgeDisplay').textContent =
        data.propertyAgeYears ? data.propertyAgeYears + ' years' : '-- years';
    document.getElementById('maintenanceLevelDisplay').textContent =
        'Maintenance: ' + (data.maintenanceLevel || '--');

    // School and CBD
    document.getElementById('schoolZoneDisplay').textContent = data.schoolZoneQuality || '--';
    document.getElementById('distanceToCbdDisplay').textContent =
        data.distanceToCbdKm ? data.distanceToCbdKm + ' km to CBD' : '-- km to CBD';
}

// Update strengths and risks lists
function updateStrengthsAndRisks(data) {
    const strengthsList = document.getElementById('strengthsList');
    const risksList = document.getElementById('risksList');

    // Clear existing items
    strengthsList.innerHTML = '';
    risksList.innerHTML = '';

    // Build strengths (good factors)
    const strengths = [];
    if (data.hasClearTitle) strengths.push('Clear property title');
    if (data.hasEncumbrances === false) strengths.push('No encumbrances on property');
    if (data.landOwnership === 'Freehold') strengths.push('Freehold ownership - full land control');
    if (data.locationCategory === 'Metro') strengths.push('Located in metro area');
    if (data.localDemand === 'High') strengths.push('High local rental demand');
    if (data.rentalYieldPercentage && data.rentalYieldPercentage >= 5) strengths.push(`Strong rental yield (${data.rentalYieldPercentage.toFixed(1)}%)`);
    if (data.rentalYieldPercentage && data.rentalYieldPercentage >= 4 && data.rentalYieldPercentage < 5) strengths.push(`Solid rental yield (${data.rentalYieldPercentage.toFixed(1)}%)`);
    if (data.capitalGrowthPercentage && data.capitalGrowthPercentage >= 5) strengths.push(`Good capital growth potential (${data.capitalGrowthPercentage.toFixed(1)}%)`);
    if (data.hasStructuralIssues === false) strengths.push('No structural issues identified');
    if (data.hasMajorDefects === false) strengths.push('No major defects');
    if (data.meetsCurrentBuildingCodes) strengths.push('Meets current building codes');
    if (data.hasRequiredCertificates) strengths.push('All required certificates in place');
    if (data.hasLongTermTenants) strengths.push('Has long-term tenants');
    if (data.hasConsistentRentalHistory) strengths.push('Consistent rental history');
    if (data.acceptedByMajorLenders) strengths.push('Accepted by major lenders');
    if (data.viableForLongTermHold) strengths.push('Viable for long-term hold');
    if (data.schoolZoneQuality === 'Top-tier') strengths.push('Top-tier school zone');
    if (data.schoolZoneQuality === 'Good') strengths.push('Good school zone quality');
    if (data.vacancyRatePercentage && data.vacancyRatePercentage < 2) strengths.push(`Low vacancy rate (${data.vacancyRatePercentage.toFixed(1)}%)`);
    if (data.distanceToCbdKm && data.distanceToCbdKm <= 10) strengths.push(`Close to CBD (${data.distanceToCbdKm} km)`);
    if (data.distanceToPublicTransportMeters && data.distanceToPublicTransportMeters <= 500) strengths.push('Close to public transport');
    if (data.propertyAgeYears && data.propertyAgeYears <= 10) strengths.push('Modern property (under 10 years)');
    if (data.maintenanceLevel === 'Minimal') strengths.push('Minimal maintenance required');
    if (data.riskRating === 'Low') strengths.push('Low risk rating');
    if (data.suitableForCrossCollateral) strengths.push('Suitable for cross-collateralization');
    if (data.eligibleForRefinance) strengths.push('Eligible for refinancing');
    if (data.hasStrongComparables) strengths.push('Strong comparable sales in area');
    if (data.fitsPortfolioDiversity) strengths.push('Good portfolio diversification');

    // Build risks (not good factors)
    const risks = [];
    if (data.hasEncumbrances) risks.push('Property has encumbrances');
    if (data.hasClearTitle === false) risks.push('Title issues may exist');
    if (data.landOwnership === 'Leasehold') risks.push('Leasehold - limited ownership period');
    if (data.landOwnership === 'Strata') risks.push('Strata - body corporate fees apply');
    if (data.hasStructuralIssues) risks.push('Structural issues identified');
    if (data.hasMajorDefects) risks.push('Major defects present');
    if (data.maintenanceLevel === 'Extensive') risks.push('Extensive maintenance required');
    if (data.maintenanceLevel === 'Moderate') risks.push('Moderate maintenance needed');
    if (data.meetsCurrentBuildingCodes === false) risks.push('May not meet current building codes');
    if (data.riskRating === 'High') risks.push('High risk rating');
    if (data.riskRating === 'Medium') risks.push('Medium risk rating');
    if (data.hasDevelopmentRisk) risks.push('Development risk in area');
    if (data.locationCategory === 'Rural') risks.push('Rural location may limit growth');
    if (data.locationCategory === 'Regional') risks.push('Regional location - slower growth');
    if (data.acceptedByMajorLenders === false) risks.push('May not be accepted by major lenders');
    if (data.isUniqueProperty) risks.push('Unique property - limited comparables');
    if (data.vacancyRatePercentage && data.vacancyRatePercentage > 5) risks.push(`High vacancy rate (${data.vacancyRatePercentage.toFixed(1)}%)`);
    if (data.vacancyRatePercentage && data.vacancyRatePercentage >= 3 && data.vacancyRatePercentage <= 5) risks.push(`Moderate vacancy rate (${data.vacancyRatePercentage.toFixed(1)}%)`);
    if (data.propertyAgeYears && data.propertyAgeYears > 40) risks.push(`Older property (${data.propertyAgeYears} years)`);
    if (data.distanceToCbdKm && data.distanceToCbdKm > 30) risks.push(`Far from CBD (${data.distanceToCbdKm} km)`);
    if (data.localDemand === 'Low') risks.push('Low local rental demand');
    if (data.rentalYieldPercentage && data.rentalYieldPercentage < 3) risks.push(`Low rental yield (${data.rentalYieldPercentage.toFixed(1)}%)`);
    if (data.capitalGrowthPercentage && data.capitalGrowthPercentage < 3) risks.push(`Low capital growth (${data.capitalGrowthPercentage.toFixed(1)}%)`);
    if (data.schoolZoneQuality === 'Average') risks.push('Average school zone');
    if (data.viableForLongTermHold === false) risks.push('Not ideal for long-term hold');

    // Add default if empty
    if (strengths.length === 0) strengths.push('Analysis completed - review detailed results');
    if (risks.length === 0) risks.push('No significant risks identified');

    // Render strengths
    strengths.slice(0, 6).forEach(strength => {
        strengthsList.innerHTML += `
            <label class="flex gap-x-3 py-2 flex-row items-start">
                <input checked class="mt-1 h-5 w-5 rounded border-[#d3e0e4] border-2 bg-transparent text-success checked:bg-success checked:border-success focus:ring-0" readonly type="checkbox"/>
                <p class="text-sm font-medium leading-normal">${strength}</p>
            </label>
        `;
    });

    // Render risks
    risks.slice(0, 4).forEach(risk => {
        risksList.innerHTML += `
            <label class="flex gap-x-3 py-2 flex-row items-start">
                <input checked class="mt-1 h-5 w-5 rounded border-[#d3e0e4] border-2 bg-transparent text-danger checked:bg-danger checked:border-danger focus:ring-0" readonly type="checkbox"/>
                <p class="text-sm font-medium leading-normal text-danger">${risk}</p>
            </label>
        `;
    });
}

// Display recent analyses
function displayRecentAnalyses(analyses) {
    const container = document.getElementById('recentAnalysesList');
    if (!container) return;

    container.innerHTML = '';

    // Sort by created date descending and take top 5
    const recent = analyses
        .sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt))
        .slice(0, 5);

    if (recent.length === 0) {
        container.innerHTML = `
            <div class="text-center py-6">
                <span class="material-symbols-outlined text-4xl text-gray-300 mb-2">history</span>
                <p class="text-gray-400 text-sm">No recent analyses</p>
                <p class="text-gray-300 text-xs mt-1">Your analysis history will appear here</p>
            </div>
        `;
        return;
    }

    recent.forEach(analysis => {
        const statusColor = analysis.status === 'Completed' ? 'success' :
                           analysis.status === 'Failed' ? 'danger' : 'primary';
        const score = analysis.analysisScore ? analysis.analysisScore.toFixed(0) : '--';
        const date = new Date(analysis.createdAt).toLocaleDateString();

        container.innerHTML += `
            <div class="flex items-center justify-between p-3 bg-background-light dark:bg-background-dark/50 rounded-lg cursor-pointer hover:bg-primary/5 transition-all" onclick="loadAnalysisById('${analysis.id}')">
                <div class="flex items-center gap-3">
                    <div class="size-10 rounded-lg bg-primary/10 flex items-center justify-center">
                        <span class="material-symbols-outlined text-primary">home</span>
                    </div>
                    <div>
                        <p class="text-sm font-bold truncate max-w-[300px]">${analysis.propertyAddress}</p>
                        <p class="text-[10px] text-gray-500">${date} - ${analysis.analyserType}</p>
                    </div>
                </div>
                <div class="flex items-center gap-2">
                    <span class="px-2 py-0.5 rounded-full bg-${statusColor}/10 text-${statusColor} text-[10px] font-black">${analysis.status}</span>
                    <span class="text-sm font-black">${score}/100</span>
                </div>
            </div>
        `;
    });
}

// Load specific analysis by ID
async function loadAnalysisById(id) {
    try {
        const response = await fetch(`${API_BASE_URL}/${id}`);
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const analysis = await response.json();
        displayAnalysis(analysis);

        // Update input field
        propertyInput.value = analysis.propertyAddress;

    } catch (error) {
        console.error('Error loading analysis:', error);
        alert('Error loading analysis: ' + error.message);
    }
}

// Set loading state
function setLoading(loading) {
    if (loading) {
        analyzeBtnText.textContent = 'Analyzing...';
        analyzeBtnSpinner.classList.remove('hidden');
        analyzeBtn.disabled = true;
        loadingState.classList.remove('hidden');
        emptyState.classList.add('hidden');
        resultsContainer.classList.add('hidden');
    } else {
        analyzeBtnText.textContent = 'Analyze';
        analyzeBtnSpinner.classList.add('hidden');
        analyzeBtn.disabled = false;
        loadingState.classList.add('hidden');
    }
}

// Export report
function exportReport() {
    if (!currentAnalysis) {
        alert('No analysis to export');
        return;
    }

    // Create text report
    let report = `PROPGIC PROPERTY ANALYSIS REPORT\n`;
    report += `================================\n\n`;
    report += `Property: ${currentAnalysis.propertyAddress}\n`;
    report += `Analysis Type: ${currentAnalysis.analyserType}\n`;
    report += `Status: ${currentAnalysis.status}\n`;
    report += `Score: ${currentAnalysis.analysisScore ? currentAnalysis.analysisScore.toFixed(1) : 'N/A'}/100\n`;
    report += `Date: ${new Date(currentAnalysis.createdAt).toLocaleString()}\n\n`;
    report += `REMARKS:\n${currentAnalysis.remarks || 'None'}\n\n`;
    report += `DETAILED RESULTS:\n${currentAnalysis.analysisResult || 'None'}\n`;

    // Download as text file
    const blob = new Blob([report], { type: 'text/plain' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `property-analysis-${currentAnalysis.id}.txt`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
}

// Share analysis
function shareAnalysis() {
    if (!currentAnalysis) {
        alert('No analysis to share');
        return;
    }

    const shareText = `Property Analysis: ${currentAnalysis.propertyAddress}\nScore: ${currentAnalysis.analysisScore ? currentAnalysis.analysisScore.toFixed(1) : 'N/A'}/100`;

    if (navigator.share) {
        navigator.share({
            title: 'Propgic Property Analysis',
            text: shareText,
            url: window.location.href
        });
    } else {
        // Fallback: copy to clipboard
        navigator.clipboard.writeText(shareText).then(() => {
            alert('Analysis summary copied to clipboard!');
        });
    }
}
