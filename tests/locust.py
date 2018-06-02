from locust import HttpLocust, TaskSet, task
class UserBehavior(BaseUserBehavior):
    tasks = {
        login: 1,
    }

    def on_start(self):
        self.onStart(config)


class ApiUser(HttpLocust):
    task_set = UserBehavior