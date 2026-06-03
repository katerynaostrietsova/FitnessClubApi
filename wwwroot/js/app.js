const apiUrl = "/api";

let clients = [];
let trainers = [];
let workoutTypes = [];
let subscriptions = [];
let workouts = [];
let registrations = [];

document.addEventListener("DOMContentLoaded", () => {
    bindForms();
    loadAllData();
});

function bindForms() {
    document.getElementById("clientForm").addEventListener("submit", createClient);
    document.getElementById("trainerForm").addEventListener("submit", createTrainer);
    document.getElementById("workoutTypeForm").addEventListener("submit", createWorkoutType);
    document.getElementById("subscriptionForm").addEventListener("submit", createSubscription);
    document.getElementById("workoutForm").addEventListener("submit", createWorkout);
    document.getElementById("registrationForm").addEventListener("submit", createRegistration);
    document.getElementById("statusForm").addEventListener("submit", updateRegistrationStatus);
}

async function loadAllData() {
    try {
        clients = await sendRequest(`${apiUrl}/Clients`);
        trainers = await sendRequest(`${apiUrl}/Trainers`);
        workoutTypes = await sendRequest(`${apiUrl}/WorkoutTypes`);
        subscriptions = await sendRequest(`${apiUrl}/Subscriptions`);
        workouts = await sendRequest(`${apiUrl}/Workouts`);
        registrations = await sendRequest(`${apiUrl}/Registrations`);

        renderClients();
        renderTrainers();
        renderWorkoutTypes();
        renderSubscriptions();
        renderWorkouts();
        renderRegistrations();
        fillSelects();
    } catch (error) {
        showMessage(error.message, true);
    }
}

async function sendRequest(url, options = {}) {
    const response = await fetch(url, {
        headers: {
            "Content-Type": "application/json"
        },
        ...options
    });

    const text = await response.text();

    if (!response.ok) {
        throw new Error(text || "Помилка запиту до API.");
    }

    if (!text) {
        return null;
    }

    try {
        return JSON.parse(text);
    } catch {
        return text;
    }
}

function renderClients() {
    const table = document.getElementById("clientsTable");

    table.innerHTML = clients.map(client => `
        <tr>
            <td>${client.id}</td>
            <td>${escapeHtml(client.fullName)}</td>
            <td>${escapeHtml(client.membershipCardNumber)}</td>
            <td>${escapeHtml(client.phone ?? "")}</td>
            <td>
                <div class="actions">
                    <button class="small-btn edit-btn" onclick="editClient(${client.id})">Редагувати</button>
                    <button class="small-btn delete-btn" onclick="deleteClient(${client.id})">Видалити</button>
                </div>
            </td>
        </tr>
    `).join("");
}

function renderTrainers() {
    const table = document.getElementById("trainersTable");

    table.innerHTML = trainers.map(trainer => `
        <tr>
            <td>${trainer.id}</td>
            <td>${escapeHtml(trainer.fullName)}</td>
            <td>${trainer.experienceYears}</td>
            <td>${escapeHtml(trainer.specialization ?? "")}</td>
            <td>
                <div class="actions">
                    <button class="small-btn edit-btn" onclick="editTrainer(${trainer.id})">Редагувати</button>
                    <button class="small-btn delete-btn" onclick="deleteTrainer(${trainer.id})">Видалити</button>
                </div>
            </td>
        </tr>
    `).join("");
}

function renderWorkoutTypes() {
    const table = document.getElementById("workoutTypesTable");

    table.innerHTML = workoutTypes.map(type => `
        <tr>
            <td>${type.id}</td>
            <td>${escapeHtml(type.name)}</td>
            <td>${type.durationMinutes}</td>
            <td>${type.isGroup ? "Так" : "Ні"}</td>
            <td>${type.requiresTrainer ? "Так" : "Ні"}</td>
            <td>
                <div class="actions">
                    <button class="small-btn edit-btn" onclick="editWorkoutType(${type.id})">Редагувати</button>
                    <button class="small-btn delete-btn" onclick="deleteWorkoutType(${type.id})">Видалити</button>
                </div>
            </td>
        </tr>
    `).join("");
}

function renderSubscriptions() {
    const table = document.getElementById("subscriptionsTable");

    table.innerHTML = subscriptions.map(subscription => `
        <tr>
            <td>${subscription.id}</td>
            <td>${escapeHtml(subscription.clientName ?? "")}</td>
            <td>${escapeHtml(subscription.workoutTypeName ?? "")}</td>
            <td>${subscription.totalSessions}</td>
            <td>${subscription.remainingSessions}</td>
            <td>${subscription.status}</td>
            <td>
                <div class="actions">
                    <button class="small-btn edit-btn" onclick="editSubscription(${subscription.id})">Редагувати</button>
                    <button class="small-btn delete-btn" onclick="deleteSubscription(${subscription.id})">Видалити</button>
                </div>
            </td>
        </tr>
    `).join("");
}

function renderWorkouts() {
    const table = document.getElementById("workoutsTable");

    table.innerHTML = workouts.map(workout => `
        <tr>
            <td>${workout.id}</td>
            <td>${escapeHtml(workout.workoutTypeName ?? "")}</td>
            <td>${escapeHtml(workout.trainerName ?? "Без тренера")}</td>
            <td>${formatDateTime(workout.workoutDateTime)}</td>
            <td>${workout.maxParticipants}</td>
            <td>${workout.status}</td>
            <td>
                <div class="actions">
                    <button class="small-btn edit-btn" onclick="editWorkout(${workout.id})">Редагувати</button>
                    <button class="small-btn delete-btn" onclick="deleteWorkout(${workout.id})">Видалити</button>
                </div>
            </td>
        </tr>
    `).join("");
}

function renderRegistrations() {
    const table = document.getElementById("registrationsTable");

    table.innerHTML = registrations.map(registration => `
        <tr>
            <td>${registration.id}</td>
            <td>${escapeHtml(registration.clientName ?? "")}</td>
            <td>${escapeHtml(registration.workoutTypeName ?? "")}</td>
            <td>${formatDateTime(registration.registrationDateTime)}</td>
            <td>${registration.status}</td>
            <td>${escapeHtml(registration.note ?? "")}</td>
            <td>
                <div class="actions">
                    <button class="small-btn delete-btn" onclick="deleteRegistration(${registration.id})">Видалити</button>
                </div>
            </td>
        </tr>
    `).join("");
}

function fillSelects() {
    fillSelect(
        "subscriptionClient",
        clients,
        client => client.id,
        client => `${client.id} — ${client.fullName}`
    );

    fillSelect(
        "registrationClient",
        clients,
        client => client.id,
        client => `${client.id} — ${client.fullName}`
    );

    fillSelect(
        "subscriptionWorkoutType",
        workoutTypes,
        type => type.id,
        type => `${type.id} — ${type.name}`
    );

    fillSelect(
        "workoutWorkoutType",
        workoutTypes,
        type => type.id,
        type => `${type.id} — ${type.name}`
    );

    fillSelect(
        "workoutTrainer",
        trainers,
        trainer => trainer.id,
        trainer => `${trainer.id} — ${trainer.fullName}`,
        "Без тренера"
    );

    fillSelect(
        "registrationWorkout",
        workouts,
        workout => workout.id,
        workout => `${workout.id} — ${workout.workoutTypeName} — ${formatDateTime(workout.workoutDateTime)}`
    );

    fillSelect(
        "registrationSubscription",
        subscriptions,
        subscription => subscription.id,
        subscription => `${subscription.id} — ${subscription.clientName} — ${subscription.workoutTypeName} — залишилось ${subscription.remainingSessions}`
    );

    fillSelect(
        "registrationStatusId",
        registrations,
        registration => registration.id,
        registration => `${registration.id} — ${registration.clientName} — ${registration.status}`
    );
}

function fillSelect(id, items, getValue, getText, emptyText = null) {
    const select = document.getElementById(id);

    let html = "";

    if (emptyText !== null) {
        html += `<option value="">${emptyText}</option>`;
    }

    html += items.map(item => `
        <option value="${getValue(item)}">${escapeHtml(getText(item))}</option>
    `).join("");

    select.innerHTML = html;
}

async function createClient(event) {
    event.preventDefault();

    const body = {
        fullName: document.getElementById("clientFullName").value,
        membershipCardNumber: document.getElementById("clientCard").value,
        phone: document.getElementById("clientPhone").value
    };

    await createEntity(`${apiUrl}/Clients`, body, "Клієнта додано.");
    event.target.reset();
}

async function createTrainer(event) {
    event.preventDefault();

    const body = {
        fullName: document.getElementById("trainerFullName").value,
        experienceYears: Number(document.getElementById("trainerExperience").value),
        specialization: document.getElementById("trainerSpecialization").value
    };

    await createEntity(`${apiUrl}/Trainers`, body, "Тренера додано.");
    event.target.reset();
}

async function createWorkoutType(event) {
    event.preventDefault();

    const body = {
        name: document.getElementById("workoutTypeName").value,
        durationMinutes: Number(document.getElementById("durationMinutes").value),
        isGroup: document.getElementById("isGroup").checked,
        requiresTrainer: document.getElementById("requiresTrainer").checked,
        description: document.getElementById("workoutTypeDescription").value
    };

    await createEntity(`${apiUrl}/WorkoutTypes`, body, "Вид тренування додано.");
    event.target.reset();
}

async function createSubscription(event) {
    event.preventDefault();

    const body = {
        clientId: Number(document.getElementById("subscriptionClient").value),
        workoutTypeId: Number(document.getElementById("subscriptionWorkoutType").value),
        totalSessions: Number(document.getElementById("totalSessions").value),
        startDate: document.getElementById("startDate").value + "T00:00:00",
        endDate: document.getElementById("endDate").value + "T00:00:00",
        price: Number(document.getElementById("price").value)
    };

    await createEntity(`${apiUrl}/Subscriptions`, body, "Абонемент додано.");
    event.target.reset();
}

async function createWorkout(event) {
    event.preventDefault();

    const trainerValue = document.getElementById("workoutTrainer").value;

    const body = {
        workoutTypeId: Number(document.getElementById("workoutWorkoutType").value),
        trainerId: trainerValue === "" ? null : Number(trainerValue),
        workoutDateTime: document.getElementById("workoutDateTime").value,
        maxParticipants: Number(document.getElementById("maxParticipants").value)
    };

    await createEntity(`${apiUrl}/Workouts`, body, "Тренування створено.");
    event.target.reset();
}

async function createRegistration(event) {
    event.preventDefault();

    const body = {
        clientId: Number(document.getElementById("registrationClient").value),
        workoutId: Number(document.getElementById("registrationWorkout").value),
        subscriptionId: Number(document.getElementById("registrationSubscription").value),
        note: document.getElementById("registrationNote").value
    };

    await createEntity(`${apiUrl}/Registrations`, body, "Клієнта записано на тренування.");
    event.target.reset();
}

async function updateRegistrationStatus(event) {
    event.preventDefault();

    const registrationId = document.getElementById("registrationStatusId").value;

    const body = {
        status: document.getElementById("newStatus").value,
        note: document.getElementById("statusNote").value
    };

    try {
        await sendRequest(`${apiUrl}/Registrations/${registrationId}/status`, {
            method: "PUT",
            body: JSON.stringify(body)
        });

        showMessage("Статус запису змінено.", false);
        event.target.reset();
        await loadAllData();
    } catch (error) {
        showMessage(error.message, true);
    }
}

async function editClient(id) {
    const client = clients.find(c => c.id === id);

    const fullName = prompt("ПІБ клієнта:", client.fullName);
    if (fullName === null) return;

    const card = prompt("Номер членської карти:", client.membershipCardNumber);
    if (card === null) return;

    const phone = prompt("Телефон:", client.phone ?? "");
    if (phone === null) return;

    await updateEntity(`${apiUrl}/Clients/${id}`, {
        fullName: fullName,
        membershipCardNumber: card,
        phone: phone
    }, "Клієнта оновлено.");
}

async function deleteClient(id) {
    await deleteEntity(`${apiUrl}/Clients/${id}`, "Видалити клієнта?");
}

async function editTrainer(id) {
    const trainer = trainers.find(t => t.id === id);

    const fullName = prompt("ПІБ тренера:", trainer.fullName);
    if (fullName === null) return;

    const experience = prompt("Досвід роботи, років:", trainer.experienceYears);
    if (experience === null) return;

    const specialization = prompt("Спеціалізація:", trainer.specialization ?? "");
    if (specialization === null) return;

    await updateEntity(`${apiUrl}/Trainers/${id}`, {
        fullName: fullName,
        experienceYears: Number(experience),
        specialization: specialization
    }, "Тренера оновлено.");
}

async function deleteTrainer(id) {
    await deleteEntity(`${apiUrl}/Trainers/${id}`, "Видалити тренера?");
}

async function editWorkoutType(id) {
    const type = workoutTypes.find(t => t.id === id);

    const name = prompt("Назва виду тренування:", type.name);
    if (name === null) return;

    const duration = prompt("Тривалість, хв:", type.durationMinutes);
    if (duration === null) return;

    const isGroupText = prompt("Групове тренування? true/false:", type.isGroup);
    if (isGroupText === null) return;

    const requiresTrainerText = prompt("Потребує тренера? true/false:", type.requiresTrainer);
    if (requiresTrainerText === null) return;

    const description = prompt("Опис:", type.description ?? "");
    if (description === null) return;

    await updateEntity(`${apiUrl}/WorkoutTypes/${id}`, {
        name: name,
        durationMinutes: Number(duration),
        isGroup: isGroupText.toLowerCase() === "true",
        requiresTrainer: requiresTrainerText.toLowerCase() === "true",
        description: description
    }, "Вид тренування оновлено.");
}

async function deleteWorkoutType(id) {
    await deleteEntity(`${apiUrl}/WorkoutTypes/${id}`, "Видалити вид тренування?");
}

async function editSubscription(id) {
    const subscription = subscriptions.find(s => s.id === id);

    const totalSessions = prompt("Усього тренувань:", subscription.totalSessions);
    if (totalSessions === null) return;

    const remainingSessions = prompt("Залишилось тренувань:", subscription.remainingSessions);
    if (remainingSessions === null) return;

    const startDate = prompt("Дата початку YYYY-MM-DD:", toDateInput(subscription.startDate));
    if (startDate === null) return;

    const endDate = prompt("Дата завершення YYYY-MM-DD:", toDateInput(subscription.endDate));
    if (endDate === null) return;

    const status = prompt("Статус: Active / Expired / Cancelled / Finished", subscription.status);
    if (status === null) return;

    const price = prompt("Ціна:", subscription.price);
    if (price === null) return;

    await updateEntity(`${apiUrl}/Subscriptions/${id}`, {
        totalSessions: Number(totalSessions),
        remainingSessions: Number(remainingSessions),
        startDate: startDate + "T00:00:00",
        endDate: endDate + "T00:00:00",
        status: status,
        price: Number(price)
    }, "Абонемент оновлено.");
}

async function deleteSubscription(id) {
    await deleteEntity(`${apiUrl}/Subscriptions/${id}`, "Видалити абонемент?");
}

async function editWorkout(id) {
    const workout = workouts.find(w => w.id === id);

    const workoutTypeId = prompt("ID виду тренування:", workout.workoutTypeId);
    if (workoutTypeId === null) return;

    const trainerId = prompt("ID тренера або порожньо, якщо без тренера:", workout.trainerId ?? "");
    if (trainerId === null) return;

    const workoutDateTime = prompt("Дата і час YYYY-MM-DDTHH:mm:", toDateTimeInput(workout.workoutDateTime));
    if (workoutDateTime === null) return;

    const maxParticipants = prompt("Кількість місць:", workout.maxParticipants);
    if (maxParticipants === null) return;

    const status = prompt("Статус: Scheduled / Cancelled / Completed", workout.status);
    if (status === null) return;

    await updateEntity(`${apiUrl}/Workouts/${id}`, {
        workoutTypeId: Number(workoutTypeId),
        trainerId: trainerId.trim() === "" ? null : Number(trainerId),
        workoutDateTime: workoutDateTime,
        maxParticipants: Number(maxParticipants),
        status: status
    }, "Тренування оновлено.");
}

async function deleteWorkout(id) {
    await deleteEntity(`${apiUrl}/Workouts/${id}`, "Видалити тренування?");
}

async function deleteRegistration(id) {
    await deleteEntity(`${apiUrl}/Registrations/${id}`, "Видалити запис на тренування?");
}

async function createEntity(url, body, successMessage) {
    try {
        await sendRequest(url, {
            method: "POST",
            body: JSON.stringify(body)
        });

        showMessage(successMessage, false);
        await loadAllData();
    } catch (error) {
        showMessage(error.message, true);
    }
}

async function updateEntity(url, body, successMessage) {
    try {
        await sendRequest(url, {
            method: "PUT",
            body: JSON.stringify(body)
        });

        showMessage(successMessage, false);
        await loadAllData();
    } catch (error) {
        showMessage(error.message, true);
    }
}

async function deleteEntity(url, confirmText) {
    if (!confirm(confirmText)) {
        return;
    }

    try {
        await sendRequest(url, {
            method: "DELETE"
        });

        showMessage("Видалено.", false);
        await loadAllData();
    } catch (error) {
        showMessage(error.message, true);
    }
}

function showMessage(text, isError) {
    const message = document.getElementById("message");

    message.textContent = text;
    message.className = isError ? "error" : "success";
    message.style.display = "block";

    setTimeout(() => {
        message.style.display = "none";
    }, 5000);
}

function formatDateTime(value) {
    if (!value) {
        return "";
    }

    return new Date(value).toLocaleString("uk-UA");
}

function toDateInput(value) {
    if (!value) {
        return "";
    }

    return value.substring(0, 10);
}

function toDateTimeInput(value) {
    if (!value) {
        return "";
    }

    return value.substring(0, 16);
}

function escapeHtml(value) {
    return String(value)
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
}