const dep = document.getElementById('department');

getDepartments().then(function (result) {
    if (result.success) {
        let arr = result.data;
        for (let i = 0; i < arr.length; i++) {
            const d = arr[i];
            let s = document.createElement('option');
            s.innerHTML = d.Name;
            s.value = d.Id;
            dep.appendChild(s);
        }
    }
});

function getAllTeachers(){
    selected_teachers = []
    getUsersByDepartmentList(parseInt(dep.value)).then((result) => {
        let users = result.data;
        users.forEach(teacher => {
            var o_api_teacher = search(teacher.Name.toLowerCase())[0];
            selected_teachers.push(o_api_teacher);
        })
    });
}

dep.addEventListener("change", async (event) => {
    getAllTeachers();
});