pushd Playbooks
export TERRAFORM_STATE_ROOT=../../Provision
ansible-playbook -i terraform.py -s update-buildservice.yml
popd
