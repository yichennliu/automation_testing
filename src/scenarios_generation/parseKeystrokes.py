import re
from pathlib import Path

from createCSV import AuditSessionData, EventData

hh_dict = {}


def split_over_the_dot(s):
    s_list = s.split('.')
    print(s_list)
    if s_list.__len__() == 3:
        return s_list[2]
    elif s_list.__len__() == 2:
        return s_list[1]


def getScenarios():
    session_count = 0
    for line in AuditSessionData:
        if "TT-000" in line[2]:
            householdID = line[2]
            householdID = householdID[:-1]
            householdID.replace(' ', '')
            if householdID not in hh_dict:
                hh_dict[householdID] = []
            session_id_str = line[0]

            session_id_stripped = session_id_str[1:-1]
            if session_id_stripped not in hh_dict.values():
                hh_dict[householdID].append(session_id_stripped)
    for hh in hh_dict:
        scenario_stack = []
        test_case = re.search('TT-000(.+?)-', hh).group(1)
        length = len(hh_dict[hh])
        for i in range(length):
            enterfield = []
            leavefield = []
            var_enter = ''
            ans = ''
            checked_var = []
            popped = []
            session = hh_dict[hh][i]
            session_count = session_count + 1;
            for line in EventData:
                if session not in line[0]:
                    continue
                else:
                    if "EnterFieldEvent" in line[3]:
                        var_enter = re.search('EnterFieldEvent FieldName="(.+?)"', line[3]).group(1)
                        enterfield.append(var_enter)
                        var_leave = ''
                        ans = ''
                    elif "LeaveFieldEvent" in line[3]:
                        var_leave = re.search('LeaveFieldEvent FieldName="(.+?)"', line[3]).group(1)
                        leavefield.append(var_leave)
                        if "Value" not in line[3]:
                            checked_var.append(var_leave)
                            ans = '1'
                        elif var_enter == var_leave:
                            try:
                                ans = re.search('Value="(.+?)"', line[3]).group(1)
                            except ValueError:
                                print("Regex value error")

                    elif "CategoryEvent" in line[3]:
                        if var_enter == var_leave:
                            scenario_stack.append(var_leave + '\t' + ans)

                    elif "NextPage()" in line[3] or "NextField()" in line[3]:
                        if var_enter == var_leave:
                            scenario_stack.append(var_leave + '\t' + ans)
                        else:
                            scenario_stack.append(var_enter + '\t' + 'next')
                    elif "PreviousPage()" in line[3]:
                        if scenario_stack:
                            val = scenario_stack.pop()
                            enterfield.pop()
                            if var_enter == var_leave:
                                if leavefield:
                                    leavefield.pop()
                            popped.append(val)

            scenario_file_name = "scenarios/scenario_" + str(session_count) + ".txt"
            with open(scenario_file_name, "a+") as f:
                f.write('Test Case' + '\t' + test_case + '\n')
                for item in scenario_stack:
                    f.write(item + '\n')
            f.close()


def main():
    for count in range(1, 11):
        scenario_file_name = "scenarios/scenario_" + str(count) + ".txt"
        my_file = Path(scenario_file_name)
        if my_file.is_file():
            open(scenario_file_name, 'w').close()
    getScenarios()


if __name__ == '__main__':
    main()
